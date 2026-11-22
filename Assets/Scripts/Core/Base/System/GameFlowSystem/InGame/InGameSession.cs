using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 单局的权威状态容器：阶段、玩家状态、运输车货物、合约奖励和结算快照。
/// 所有写入只在服务端进行；客户端和晚加入者从网络变量恢复，不依赖一次性 RPC。
/// </summary>
public class InGameSession : NetworkBehaviour
{
    [SerializeField, Tooltip("本局规则配置")]
    private MatchConfig m_Config;

    private readonly NetworkList<InGamePlayerData> m_Players = new();
    private readonly NetworkList<PlayerResultData> m_Results = new();
    private readonly NetworkList<CargoEntry> m_Cargo = new();
    private readonly NetworkList<int> m_ContractIds = new();
    public NetworkVariable<bool> PlayersSpawned = new();
    private readonly NetworkVariable<MatchPhase> m_Phase = new();
    private readonly NetworkVariable<MatchEndReason> m_EndReason = new();
    private readonly NetworkVariable<int> m_CargoValue = new();
    private readonly NetworkVariable<int> m_ContractReward = new();
    private readonly NetworkVariable<int> m_RansomTotal = new();

    public MatchConfig Config => m_Config;
    public NetworkList<InGamePlayerData> Players => m_Players;
    public NetworkList<PlayerResultData> Results => m_Results;
    public NetworkList<CargoEntry> Cargo => m_Cargo;
    public MatchPhase Phase => m_Phase.Value;
    public MatchEndReason EndReason => m_EndReason.Value;
    public int CargoValue => m_CargoValue.Value;
    public int ContractReward => m_ContractReward.Value;
    public int RansomTotal => m_RansomTotal.Value;
    public double ServerTimeNow => NetworkManager.ServerTime.Time;

    public override void OnNetworkSpawn()
    {
        DontDestroyOnLoad(gameObject);
        GameRoot.Instance.GameFlow.InGameSystem.RegisterSession(this);
    }

    public override void OnNetworkDespawn()
    {
        GameRoot.Instance.GameFlow.InGameSystem.UnregisterSession(this);
    }

    // ======================== 查询 ========================

    public bool TryGetPlayerIndex(ulong clientId, out int index)
    {
        for (var i = 0; i < m_Players.Count; i++)
        {
            if (m_Players[i].ClientId == clientId)
            {
                index = i;
                return true;
            }
        }

        index = -1;
        return false;
    }

    /// <summary>不在本局名单里的玩家按 Left 处理。</summary>
    public PlayerMatchStatus GetStatus(ulong clientId)
    {
        return TryGetPlayerIndex(clientId, out var index) ? m_Players[index].Status : PlayerMatchStatus.Left;
    }

    public void CountStatuses(out int free, out int aboard, out int captured)
    {
        free = 0;
        aboard = 0;
        captured = 0;
        for (var i = 0; i < m_Players.Count; i++)
        {
            switch (m_Players[i].Status)
            {
                case PlayerMatchStatus.Free: free++; break;
                case PlayerMatchStatus.Aboard: aboard++; break;
                case PlayerMatchStatus.Captured: captured++; break;
            }
        }
    }

    public void CopyContractIds(List<int> buffer)
    {
        foreach (var id in m_ContractIds)
        {
            buffer.Add(id);
        }
    }

    // ======================== 服务端写入 ========================

    public void ServerSetPlayers(List<InGamePlayerData> players)
    {
        if (!IsServer)
        {
            return;
        }

        m_Players.Clear();
        foreach (var player in players)
        {
            m_Players.Add(player);
        }
    }

    /// <summary>开局前由服务端写入大厅里接取的合约，开局后不可更改。</summary>
    public void ServerSetContracts(List<int> contractIds)
    {
        if (!IsServer || m_Phase.Value != MatchPhase.Waiting)
        {
            return;
        }

        m_ContractIds.Clear();
        foreach (var id in contractIds)
        {
            m_ContractIds.Add(id);
        }
    }

    public void ServerMarkSpawned()
    {
        if (IsServer)
        {
            PlayersSpawned.Value = true;
        }
    }

    /// <summary>只允许从 Waiting 进入，重复调用无效。</summary>
    public void ServerBeginMatch()
    {
        if (IsServer && m_Phase.Value == MatchPhase.Waiting)
        {
            m_Phase.Value = MatchPhase.Playing;
        }
    }

    public void ServerSetStatus(ulong clientId, PlayerMatchStatus status)
    {
        if (!IsServer || !TryGetPlayerIndex(clientId, out var index))
        {
            return;
        }

        var data = m_Players[index];
        if (data.Status == status)
        {
            return;
        }

        data.Status = status;
        m_Players[index] = data;
    }

    public void ServerAddCargo(uint itemId, int count, int value)
    {
        if (!IsServer || m_Phase.Value != MatchPhase.Playing || count <= 0)
        {
            return;
        }

        var merged = false;
        for (var i = 0; i < m_Cargo.Count; i++)
        {
            var entry = m_Cargo[i];
            if (entry.ItemId != itemId)
            {
                continue;
            }

            entry.Count += count;
            m_Cargo[i] = entry;
            merged = true;
            break;
        }

        if (!merged)
        {
            m_Cargo.Add(new CargoEntry { ItemId = itemId, Count = count });
        }

        m_CargoValue.Value += value;
    }

    public void ServerAddContractReward(int amount)
    {
        if (IsServer && amount > 0 && m_Phase.Value == MatchPhase.Playing)
        {
            m_ContractReward.Value += amount;
        }
    }

    /// <summary>
    /// 写入结算快照并结束对局。只允许从 Playing 进入，
    /// 因此重复触发不会产生第二份结算。
    /// </summary>
    public void ServerEndMatch(MatchEndReason reason, List<PlayerResultData> results, int ransomTotal)
    {
        if (!IsServer || m_Phase.Value != MatchPhase.Playing)
        {
            return;
        }

        m_Results.Clear();
        foreach (var result in results)
        {
            m_Results.Add(result);
        }

        m_RansomTotal.Value = ransomTotal;
        m_EndReason.Value = reason;
        m_Phase.Value = MatchPhase.Ended;
    }
}