using System.Collections.Generic;

/// <summary>
/// 对局编排（仅服务端生效）：开局、发车判定、失败判定和收尾。
/// 所有非囚禁的猫上车后立即结算，没有确认倒计时。
/// </summary>
public class MatchSystem : SubSystem
{
    private const string Tag = "MatchSystem";
    private readonly GamePlaySystem m_GamePlay;
    private readonly List<int> m_ContractBuffer = new();
    private bool m_IsServerRunning;

    public MatchSystem(ISystem mainSystem) : base(mainSystem)
    {
        m_GamePlay = (GamePlaySystem)mainSystem;
    }

    private static InGameSession Session => GameRoot.Instance.GameFlow.InGameSystem.Session;
    private static NetworkSystem Net => GameRoot.Instance.GameNet;

    /// <summary>由服务端的 InGameState 调用。Phase 不是 Waiting 时直接返回，保证幂等。</summary>
    public void ServerStartMatch()
    {
        var session = Session;
        if (!Net.IsServer || session == null || session.Phase != MatchPhase.Waiting)
        {
            return;
        }

        if (session.Config == null)
        {
            Log.Error(Tag, "InGameSession 未配置 MatchConfig，无法开局");
            return;
        }

        session.ServerBeginMatch();
        Net.OnClientDisconnected += OnClientDisconnected;
        m_IsServerRunning = true;
        m_GamePlay.CaptureSystem.ServerReset();

        // 带入 0 份合约就是盗窃模式，不需要额外的模式分支
        m_ContractBuffer.Clear();
        session.CopyContractIds(m_ContractBuffer);
        m_GamePlay.TaskSystem.ServerStartContracts(m_ContractBuffer);
    }

    public override void OnUpdate(float deltaTime)
    {
        if (!m_IsServerRunning)
        {
            return;
        }

        var session = Session;
        if (session == null || session.Phase != MatchPhase.Playing)
        {
            return;
        }

        session.CountStatuses(out var free, out var aboard, out var captured);
        if (free > 0)
        {
            // 还有自由玩家没上车就不发车
            return;
        }

        if (aboard == 0)
        {
            // 没有人在车上也没有人能上车：全员被关押或全员离开
            ServerEndMatch(captured > 0 ? MatchEndReason.AllCaptured : MatchEndReason.AllLeft);
            return;
        }

        // 所有非囚禁的猫都在车上，立即结算
        ServerEndMatch(MatchEndReason.Departed);
    }

    private void ServerEndMatch(MatchEndReason reason)
    {
        var session = Session;
        if (session == null || session.Phase != MatchPhase.Playing)
        {
            return;
        }

        StopServerLoop();
        m_GamePlay.TaskSystem.ServerStop();

        var results = GameRoot.Instance.GameFlow.ResultSystem.ServerBuildResults(session, out var ransomTotal);
        session.ServerEndMatch(reason, results, ransomTotal);

        // 快照已固化，回收所有角色，避免结算界面期间还能移动或拾取
        var players = session.Players;
        for (var i = 0; i < players.Count; i++)
        {
            var player = PlayerMgr.Instance.GetPlayer(players[i].ClientId);
            if (player != null && player.IsSpawned)
            {
                player.NetworkObject.Despawn(true);
            }
        }

        m_GamePlay.CaptureSystem.ServerReset();
    }

    private void OnClientDisconnected(ulong clientId)
    {
        var session = Session;
        if (session == null || session.Phase != MatchPhase.Playing)
        {
            return;
        }

        // 掉线玩家不再阻塞发车，也不参与结算
        session.ServerSetStatus(clientId, PlayerMatchStatus.Left);
        m_GamePlay.CaptureSystem.ServerForget(clientId);
    }

    private void StopServerLoop()
    {
        m_IsServerRunning = false;
        Net.OnClientDisconnected -= OnClientDisconnected;
    }

    public override void OnDispose()
    {
        if (m_IsServerRunning)
        {
            StopServerLoop();
        }
    }
}