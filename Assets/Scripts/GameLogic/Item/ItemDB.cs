using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Item/Item Database")]
public class ItemDB : ScriptableObject
{
    public List<ItemData> Items = new();
    private Dictionary<uint, ItemData> m_ItemDictById;
    private Dictionary<ItemType, List<ItemData>> m_ItemDictByType;

    public void Init()
    {
        m_ItemDictById = new Dictionary<uint, ItemData>(Items.Count);
        m_ItemDictByType = new Dictionary<ItemType, List<ItemData>>();
        foreach (var item in Items)
        {
            if (item == null) continue;
            if (!m_ItemDictById.TryAdd(item.Id, item))
            {
                Debug.LogError($"[ItemDB] 重复的 ItemId: {item.Id}", this);
                continue;
            }
        }
    }

    public ItemData GetItemById(uint itemId)
    {
        if (m_ItemDictById == null) Init();
        if (m_ItemDictById == null)
        {
            return null;
        }
        m_ItemDictById.TryGetValue(itemId, out var item);
        return item;
    }
}