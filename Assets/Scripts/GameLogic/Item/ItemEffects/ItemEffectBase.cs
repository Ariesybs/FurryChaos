using System;
/// <summary>
/// 道具效果基类
/// </summary>
[Serializable]
public abstract class ItemEffectBase
{
    public abstract void ServerApply();
}