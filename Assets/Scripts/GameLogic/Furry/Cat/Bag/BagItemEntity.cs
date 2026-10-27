using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[Serializable]
public struct BagItemEntity : INetworkSerializable,IEquatable<BagItemEntity>
{
    public int ItemId;
    public int ItemCount;
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        
    }

    public bool Equals(BagItemEntity other)
    {
        return this.ItemId == other.ItemId;
    }
}
