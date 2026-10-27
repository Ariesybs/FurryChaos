using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{
    public string GetPrompt(); // 获取可交互物提示
    public bool CanInteract(ulong netEntityId); // 是否可交互
    public InteractType InteractType { get; } // 交互方式
    public void OnPressInteract(ulong netEntityId);
    public void OnHoldInteract(ulong netEntityId);
}

public enum InteractType
{
    [InspectorName("按下")]
    Press,
    [InspectorName("长按")]
    Hold
}
