using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleEx : Toggle
{
    protected override void Reset()
    {
        base.Reset();
        var toggleGroup = transform.parent.GetComponent<ToggleGroup>();
        if (toggleGroup != null)
        {
            group = toggleGroup;
        }
        
    }
}
