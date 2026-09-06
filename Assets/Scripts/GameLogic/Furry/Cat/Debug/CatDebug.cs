using System;
using KinematicCharacterController;
using UnityEngine;

public class CatDebug : MonoBehaviour
{
    public KinematicCharacterMotor CatMotor;
    public CatAnimancer CatAnimancer; 
    private bool slowDown = false;
    private GUIStyle labelStyle; // 缓存样式
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            slowDown = !slowDown;
            Time.timeScale = slowDown ? 0.2f : 1f;
        }
    }

    private void OnGUI()
    {
        if (CatMotor == null) return;
        // GUIStyle 只能在 OnGUI 里初始化
        if (labelStyle == null)
        {
            labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 36, // 字体大小，默认是 12 左右
                normal = { textColor = Color.white }
            };
        }
        Vector3 velocity = CatMotor.Velocity;
        float planarSpeed = new Vector2(velocity.x, velocity.z).magnitude;
        GUI.Label(new Rect(10, 10, 500, 50),
            $"Velocity: {velocity:F2}", labelStyle);
        GUI.Label(new Rect(10, 45, 500, 50),
            $"Speed: {velocity.magnitude:F2}  Planar: {planarSpeed:F2}", labelStyle);
        
        if (CatAnimancer != null)
        {
            GUI.Label(new Rect(10, 90, 500, 50),
                $"Target: {CatAnimancer.TargetParameter:F2}", labelStyle);
            GUI.Label(new Rect(10, 135, 500, 50),
                $"Smoothed: {CatAnimancer.SmoothedParameter:F2}", labelStyle);
        }
    }
    
    private void OnDrawGizmos()
    {
        Vector3 center = transform.position;
        float radius = 1f;
        int segments = 32;

        // 画圆
        Vector3 prev = center + new Vector3(radius, 0, 0);
        for (int i = 1; i <= segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;
            Vector3 next = center + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;
            Gizmos.DrawLine(prev, next);
            prev = next;
        }

        // 画方向箭头（用 Mixer 的本地参数转成世界方向）
        Vector2 param = CatAnimancer.SmoothedParameter;
        Vector3 dir = transform.TransformDirection(new Vector3(param.x, 0, param.y));
        Gizmos.color = Color.green;
        Gizmos.DrawLine(center, center + dir.normalized * radius);
    }
}