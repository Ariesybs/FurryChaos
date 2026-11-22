// Assets/Scripts/Core/Base/System/GamePlaySystem/TaskSystem/TaskAward.cs
using System;

/// <summary>合约奖励的静态配置。</summary>
[Serializable]
public abstract class TaskAward
{
    /// <summary>折算成金币的数额，合约完成时计入结算总池。</summary>
    public abstract int GetCoinReward();
}