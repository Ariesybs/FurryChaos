using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class LandingSurface : MonoBehaviour
{
    public Collider m_SurfaceCollider;
    public uint m_SurfaceId;
    
    // 注册时缓存的世界空间包围球（静态物体只需算一次）
    public Vector3 BoundsCenter { get; private set; }
    public float BoundsRadius { get; private set; }
    
    protected virtual void OnEnable()
    {
        if (m_SurfaceCollider == null)
        {
            return;
        }
        var b = m_SurfaceCollider.bounds;
        BoundsCenter = b.center;
        BoundsRadius = b.extents.magnitude;
        LandingSurfaceRegistry.Register(this);
    }

    public abstract Vector3 GetClosestStandPoint(Vector3 wordPos);

#if UNITY_EDITOR
    public abstract void Highlight();
#endif

    protected virtual void OnDisable()
    {
        if (m_SurfaceCollider != null)
        {
            LandingSurfaceRegistry.Unregister(this);
        }
    }
}
