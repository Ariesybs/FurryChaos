using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 打碎家具
/// </summary>
public class BrokenFurniture : TaskUnit
{
    public override void Init()
    {
        
    }

    public override bool Done()
    {
        return false;
    }

    public override float GetProgress()
    {
        return 0;
    }

    public override string GetProgressStr()
    {
        return "1/2";
    }
}
