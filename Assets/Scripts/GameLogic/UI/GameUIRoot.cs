using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameUIRoot : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(this);
    }
}
