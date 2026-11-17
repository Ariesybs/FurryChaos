using System;using Unity.Netcode;

public class BagComponent : NetworkBehaviour
{
    public readonly NetworkList<SlotData> Slots = new(
        readPerm: NetworkVariableReadPermission.Everyone, // 所有人可读
        writePerm: NetworkVariableWritePermission.Server // 服务器写入
    );

    public int MaxWeight = 3000; // 3KG
    public int SlotMaxCount = 64;
    
    public float UsedWeight
    {
        get
        {
            var sum = 0f;
            foreach (var slot in Slots)
            {
                sum += slot.GetWight();
            }
            return sum;
        }
    }

    public float FreeWeight => MaxWeight - UsedWeight;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
        {
            for (byte i = 0; i < SlotMaxCount; i++)
            {
                var slot = new SlotData()
                {
                    SlotId = i
                };
                slot.SetState(SlotData.State.Empty);
                Slots.Add(slot);
            }
        }
    }

    public SlotData GetFirstEmptySlot(out int idx)
    {
        idx = -1;
        for (var i = 0; i < Slots.Count; i++)
        {
            var slot = Slots[i];
            if (slot.IsEmpty())
            {
                idx = i;
                return slot;
            }
        }

        return default;
    }

    public void TryAddItemToBag(NetworkBehaviourReference item)
    {
        RequestAddItemServerRpc(item);
    }

    [ServerRpc]
    private void RequestAddItemServerRpc(NetworkBehaviourReference itemRef)
    {
        if (!IsServer)
        {
            return;
        }

        if (itemRef.TryGet(out Item item) && CheckEnablePick(item))
        {
            var itemId = item.data.Id;
            if (OwnedItem(itemId))
            {
                ModifySlot(itemId, d => { d.Count++; return d; });
            }
            else
            {
                var slot = GetFirstEmptySlot(out var idx);
                if (idx != -1)
                {
                    slot.Count = 1;
                    slot.ItemId = itemId;
                    slot.SetState(SlotData.State.Used);
                    ModifySlot(idx,slot);
                }
            }
            
            item.Dispose();
        }
    }
    
    public void ModifySlot(uint itemId, Func<SlotData, SlotData> modifier)
    {
        if (!IsServer) return;
    
        for (var i = 0; i < Slots.Count; i++)
        {
            if (Slots[i].ItemId == itemId)
            {
                var oldData = Slots[i];
                var newData = modifier(oldData);  
                Slots[i] = newData;
                return;
            }
        }
        return;
    }

    public void ModifySlot(int idx, SlotData newData)
    {
        if (!IsServer) return;
        if (idx >= 0 && idx < Slots.Count)
        {
            Slots[idx] = newData;
        }
    }

    public bool OwnedItem(uint itemId)
    {
        foreach (var slot in Slots)
        {
            if (slot.ItemId == itemId)
            {
                return true;
            }
        }

        return false;
    }

    public bool CheckEnablePick(Item item)
    {
        return FreeWeight >= item.data.Weight;
    }
    

    public int GetItemTotalWeight(uint itemId)
    {
        var weight = 0;
        foreach (var slot in Slots)
        {
            if (!slot.IsEmpty() && slot.ItemId == itemId)
            {
                weight = slot.GetWight();
            }
        }
        return weight;
    }
    
}

public struct BagItemNetData : INetworkSerializable, IEquatable<BagItemNetData>
{
    public int InstanceId;   // 实例唯一ID,服务器分配,背包内唯一
    public uint ItemId;       // 指向 ItemData(静态配置)
    public float weight;
    public int Flags;         // 位标记:是否已使用、是否绑定等

    public void NetworkSerialize<T>(BufferSerializer<T> s) where T : IReaderWriter
    {
        s.SerializeValue(ref InstanceId);
        s.SerializeValue(ref ItemId);
        s.SerializeValue(ref weight);
        s.SerializeValue(ref Flags);
    }

    public bool Equals(BagItemNetData o) =>
        InstanceId == o.InstanceId && ItemId == o.ItemId;
}

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