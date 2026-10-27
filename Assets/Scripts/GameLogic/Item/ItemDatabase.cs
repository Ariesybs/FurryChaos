using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Item/Item Database")]
public class ItemDatabase : ScriptableObject
{
    public List<ItemData> Items = new();
    private Dictionary<int, ItemData> m_ItemDictById;
    private Dictionary<ItemType, ItemData> m_ItemDictByType;
    public ItemData GetItemById(int itemId)
    {
        m_ItemDictById.TryGetValue(itemId,out var item);
        return item;
    }
}