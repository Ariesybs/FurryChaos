using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;

public enum MatchPhase : byte
{
    Waiting = 0,
    Playing = 1,
    Ended = 2,
}

public enum MatchEndReason : byte
{
    None = 0,
    Departed = 1,    // 运输车发车
    AllCaptured = 2, // 没有人能上车
    AllLeft = 3,     // 所有玩家都离开了
}

/// <summary>运输车上的一类货物。</summary>
public struct CargoEntry : INetworkSerializable, IEquatable<CargoEntry>
{
    public uint ItemId;
    public int Count;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ItemId);
        serializer.SerializeValue(ref Count);
    }

    public bool Equals(CargoEntry other) => ItemId == other.ItemId && Count == other.Count;
}

/// <summary>单个玩家的结算结果，由服务端生成后复制给所有人。</summary>
public struct PlayerResultData : INetworkSerializable, IEquatable<PlayerResultData>
{
    public ulong ClientId;
    public FixedString64Bytes PlayerName;
    public PlayerMatchStatus Status;
    public int Share;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref PlayerName);
        serializer.SerializeValue(ref Status);
        serializer.SerializeValue(ref Share);
    }

    public bool Equals(PlayerResultData other)
    {
        return ClientId == other.ClientId && Status == other.Status &&
               Share == other.Share && PlayerName.Equals(other.PlayerName);
    }
}

/// <summary>本地保存的结算副本，结算界面读取它，不依赖网络对象的生命周期。</summary>
public sealed class MatchResult
{
    public MatchEndReason Reason;
    public int CargoValue;
    public int ContractReward;
    public int RansomTotal;
    public bool HasEscaped;
    public readonly List<PlayerResultData> Players = new();

    /// <summary>扣除赎金后可分配的金额；无人撤离时为 0。</summary>
    public int NetValue => HasEscaped ? Math.Max(0, CargoValue + ContractReward - RansomTotal) : 0;
}