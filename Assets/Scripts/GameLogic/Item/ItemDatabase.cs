using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Item/Item Database")]
public class ItemDB : ScriptableObject
{
    public List<ItemData> Items = new();
    private readonly Dictionary<uint, ItemData> m_ItemDictById = new();
    private Dictionary<ItemType, ItemData> m_ItemDictByType;

    public ItemDB()
    {
        if (Items.Count > 0)
        {
            foreach (var item in Items)
            {
                m_ItemDictById.Add(item.Id,item);
            }
        }
    }

    public ItemData GetItemById(uint itemId)
    {
        m_ItemDictById.TryGetValue(itemId,out var item);
        return item;
    }
}