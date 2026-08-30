using System;
using UnityEngine;

public class CatDebug : MonoBehaviour
{
    private bool slowDown = false;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            slowDown = !slowDown;
            Time.timeScale = slowDown ? 0.2f : 1f;
        }
    }
}