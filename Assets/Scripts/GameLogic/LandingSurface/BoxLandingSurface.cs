using System;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class BoxLandingSurface : LandingSurface
{
    private void Awake()
    {
        m_SurfaceCollider = GetComponent<BoxCollider>();
    }

    public override Vector3 GetClosestStandPoint(Vector3 wordPos)
    {
        // 世界坐标 -> 局部坐标
        var local = transform.InverseTransformPoint(wordPos);
        var surface = m_SurfaceCollider as BoxCollider;
        if (surface == null)
        {
            return Vector3.zero;
        }
        var size = surface.size;
        var center = surface.center;
        
        var halfX = size.x / 2;
        var halfZ = size.z / 2;
        var topY = center.y + size.y / 2;
        
        // 投影Clam
        local.x = Mathf.Clamp(local.x, center.x - halfX, center.x + halfX);
        local.z = Mathf.Clamp(local.z, center.z - halfZ, center.z + halfZ);
        local.y = topY;
        
        // 局部坐标 -> 世界坐标
        var word = transform.TransformPoint(local);
        return word;
    }

    public override void Highlight()
    {
        
    }
}