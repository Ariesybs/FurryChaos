using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UILoadingCtrl : UIPanel
{
    public override UIDisplayMode DisplayMode => UIDisplayMode.FullScreen;
    public Slider m_Slider;
    void Start()
    {
        
    }

    public void SetProgress(float progress)
    {
        if (m_Slider != null)
        {
            m_Slider.value = progress;
        }
    }

}
