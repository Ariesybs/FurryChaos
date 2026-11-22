using System;
using System.Collections.Generic;
using Unity.Netcode;

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
    
    // Assets/Scripts/GameLogic/Furry/Cat/Bag/BagComponent.cs
    /// <summary>
    /// 服务端：移除某种物品的全部数量，返回移除的总重量(g)。
    /// 交付必须走这里真正扣除，否则重复交互可以刷任务进度。
    /// </summary>
    public int ServerRemoveAllOf(uint itemId)
    {
        if (!IsServer)
        {
            return 0;
        }

        for (var i = 0; i < Slots.Count; i++)
        {
            var slot = Slots[i];
            if (slot.IsEmpty() || slot.ItemId != itemId)
            {
                continue;
            }

            var weight = slot.GetWight();
            slot.ItemId = 0;
            slot.Count = 0;
            slot.SetState(SlotData.State.Empty);
            Slots[i] = slot;
            return weight; // 同种物品只占一个槽位
        }

        return 0;
    }

    /// <summary>服务端：统计当前携带物的数量与总价值，用于撤离时生成快照。</summary>
    public void ServerGetCarrySummary(out int itemCount, out int totalValue)
    {
        itemCount = 0;
        totalValue = 0;
        foreach (var slot in Slots)
        {
            if (slot.IsEmpty())
            {
                continue;
            }

            var item = GameData.ItemDB.GetItemById(slot.ItemId);
            if (item == null)
            {
                continue;
            }

            itemCount += slot.Count;
            totalValue += item.Price * slot.Count;
        }
    }
    
    public void ServerTakeLoot(List<SlotData> output)
    {
        if (!IsServer)
        {
            return;
        }
        for (var i = 0; i < Slots.Count; i++)
        {
            var slot = Slots[i];
            if (slot.IsEmpty())
            {
                continue;
            }
            var item = GameData.ItemDB.GetItemById(slot.ItemId);
            if (item == null || item.LootValue <= 0)
            {
                continue; // 增益道具等非赃物留在背包
            }
            output.Add(slot);
            slot.ItemId = 0;
            slot.Count = 0;
            slot.SetState(SlotData.State.Empty);
            Slots[i] = slot;
        }
    }
}