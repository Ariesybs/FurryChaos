using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class UIRaycastReceiver : Graphic
{
    public override void SetMaterialDirty() { }
    public override void SetVerticesDirty() { }
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear(); // 不生成网格 = 看不见
    }
}