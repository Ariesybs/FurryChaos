using System;
using Unity.Collections;
using Unity.Netcode;

public struct LobbyPlayerData : INetworkSerializable, IEquatable<LobbyPlayerData>
{
    public ulong ClientId;
    public FixedString64Bytes PlayerName;
    public bool IsLobbyOwner; // 是否是房主
    public bool IsReady;
    public FixedString64Bytes CharacterName;
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref PlayerName);
        serializer.SerializeValue(ref IsReady);
        serializer.SerializeValue(ref IsLobbyOwner);
        serializer.SerializeValue(ref CharacterName);
    }

    public bool Equals(LobbyPlayerData other)
    {
        return ClientId == other.ClientId;
    }
}