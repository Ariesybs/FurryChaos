using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public enum ItemType
{
    [LabelText("增益道具")]
    Buff, // 增益道具
    [LabelText("任务道具")]
    Task, // 任务道具
    [LabelText("装饰道具")]
    Disguise, // 装扮道具
}

[CreateAssetMenu(menuName = "Game/Item/Item Data")]
public class ItemData : ScriptableObject
{
    [LabelText("道具ID")]
    public uint Id;
    [LabelText("道具名称")]
    public string Name;
    [LabelText("道具图标")]
    public Sprite Icon;
    [LabelText("道具类型")]
    public ItemType Type;
    [LabelText("道具描述")]
    [TextArea]
    public string Description;
    [LabelText("是否可消耗")]
    public bool Consumable; // 是否可消耗
    
    [Header("商城相关")]
    [LabelText("是否上架商城")]
    public bool EnableList = true; // 是否可上架商城
    [LabelText("道具价格")]
    public int Price;
    [LabelText("道具重量")]
    public int Weight; // 重量
    
    [Header("效果列表")]
    [SerializeReference]
    public List<ItemEffectBase> Effects = new();
}
