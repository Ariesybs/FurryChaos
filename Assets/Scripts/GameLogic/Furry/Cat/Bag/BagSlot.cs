using System;
using Unity.Netcode;

public struct SlotData : INetworkSerializable, IEquatable<SlotData>
{
    public byte SlotId;
    public uint ItemId;
    public ushort Count;
    public byte Flags;

    public enum State : byte
    {
        Empty = 1 << 0,
        Used = 1 << 1,
        Locked = 1 << 2,
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref SlotId);
        serializer.SerializeValue(ref ItemId);
        serializer.SerializeValue(ref Count);
        serializer.SerializeValue(ref Flags);
    }

    public bool IsEmpty()
    {
        return Flags == (byte)State.Empty;
    }

    /// <summary>
    /// 返回当前槽位物品的携带重量,单位g
    /// </summary>
    /// <returns></returns>
    public int GetWight()
    {
        var weight = 0;
        if (Flags == (byte)State.Used)
        {
            var item = GameData.ItemDB.GetItemById(ItemId);
            if (item != null)
            {
                weight = item.Weight * Count;
            }
        }

        return weight;
    }

    public void SetState(State state)
    {
        Flags = (byte)state;
    }

    public State GetState()
    {
        return (State)Flags;
    }

    public bool Equals(SlotData other)
    {
        return SlotId == other.SlotId && ItemId == other.ItemId &&
               Count == other.Count && Flags == other.Flags;
    }
}