using System;
using UnityEngine;

public class LandingSurfaceScanner : MonoBehaviour
{
    private Collider[] m_Results;

    private void Awake()
    {
        m_Results = new Collider[10];
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            Scan();
        }
    }

    private void Scan()
    {
        var count = Physics.OverlapSphereNonAlloc(transform.position, 5f, m_Results);
        for (int i = 0; i < count; i++)
        {
            var c = m_Results[i];
            if(c == null) continue;
            if (c.TryGetComponent<LandingSurface>(out var surface))
            {
                var point = surface.GetClosestStandPoint(transform.position);
                ViewDebug.DrawPoint(point,0.1f,Color.green,10f);
            }
            
        }
    }
}