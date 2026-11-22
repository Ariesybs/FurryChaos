using System.Collections.Generic;
using UnityEngine;

public class ResultSystem : SubSystem
{
    public MatchResult LastResult { get; private set; }

    public ResultSystem(ISystem mainSystem) : base(mainSystem)
    {
    }

    /// <summary>
    /// 服务端：只读取最终权威状态生成结算。
    /// 人均 = max(0, 车上货值 + 合约奖励 − 赎金 × 未上车人数) ÷ 参与人数。
    /// 被捕者也参与平分：赎金是全队共同损失，不能让队友被抓反而使其他人获利。
    /// </summary>
    public List<PlayerResultData> ServerBuildResults(InGameSession session, out int ransomTotal)
    {
        session.CountStatuses(out var free, out var aboard, out var captured);
        // 正常发车时不会有自由玩家；防御性地把没上车的都算作被留下
        var leftBehind = free + captured;
        var participants = aboard + leftBehind;

        ransomTotal = session.Config.RansomPerPlayer * leftBehind;
        var pool = session.CargoValue + session.ContractReward;
        // 没有任何人随车离开时货物带不走，按 0 结算；总池不够付赎金时也按 0，不出现负数
        var net = aboard > 0 ? Mathf.Max(0, pool - ransomTotal) : 0;
        var share = participants > 0 ? net / participants : 0;

        var players = session.Players;
        var results = new List<PlayerResultData>(players.Count);
        for (var i = 0; i < players.Count; i++)
        {
            var player = players[i];
            results.Add(new PlayerResultData
            {
                ClientId = player.ClientId,
                PlayerName = player.PlayerName,
                Status = player.Status,
                Share = player.Status == PlayerMatchStatus.Left ? 0 : share
            });
        }

        return results;
    }

    /// <summary>各端进入结算状态时调用，把复制来的结算数据拷贝到本地。</summary>
    public MatchResult CaptureFromSession(InGameSession session)
    {
        if (session == null)
        {
            return LastResult;
        }

        var result = new MatchResult
        {
            Reason = session.EndReason,
            CargoValue = session.CargoValue,
            ContractReward = session.ContractReward,
            RansomTotal = session.RansomTotal
        };

        foreach (var player in session.Results)
        {
            result.Players.Add(player);
            result.HasEscaped |= player.Status == PlayerMatchStatus.Aboard;
        }

        LastResult = result;
        // TODO: 存档系统落地后，在这里把本地玩家的 Share 入账；
        // 需要给每局分配唯一 Id 并记录已入账的局，保证重复进入结算不会重复发奖。
        return result;
    }
}