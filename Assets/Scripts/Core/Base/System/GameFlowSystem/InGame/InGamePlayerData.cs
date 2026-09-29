using System;
using Unity.Collections;
using Unity.Netcode;

public struct InGamePlayerData : INetworkSerializable,IEquatable<InGamePlayerData>
{
    public ulong ClientId;
    public FixedString64Bytes PlayerName;
    public FixedString64Bytes CharacterName;
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref PlayerName);
        serializer.SerializeValue(ref CharacterName);
    }
    

    public bool Equals(InGamePlayerData other)
    {
        return ClientId == other.ClientId;
    }
}