using System;
using Unity.Collections;
using Unity.Netcode;

/// <summary>玩家在局内的状态，只由服务端写入。</summary>
public enum PlayerMatchStatus : byte
{
    Free = 0,     // 自由行动
    Aboard = 1,   // 已上运输车
    Captured = 2, // 被关押，可被队友营救
    Left = 3,     // 中途离开或掉线
}

public struct InGamePlayerData : INetworkSerializable, IEquatable<InGamePlayerData>
{
    public ulong ClientId;
    public FixedString64Bytes PlayerName;
    public FixedString64Bytes CharacterName;
    public PlayerMatchStatus Status;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref PlayerName);
        serializer.SerializeValue(ref CharacterName);
        serializer.SerializeValue(ref Status);
    }

    public bool Equals(InGamePlayerData other)
    {
        return ClientId == other.ClientId &&
               Status == other.Status &&
               PlayerName.Equals(other.PlayerName) &&
               CharacterName.Equals(other.CharacterName);
    }
}