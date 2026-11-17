using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class UIBagInfoCtrl : MonoBehaviour
{
    [SerializeField] private List<UIBagItemCtrl> m_BagItemCtrlList = new();

    private Player m_MySelfPlayer;
    
    // Start is called before the first frame update
    void Start()
    {
        m_MySelfPlayer = PlayerMgr.Instance.GetSelf();
        if (m_MySelfPlayer != null && m_MySelfPlayer.Bag != null)
        {
            m_MySelfPlayer.Bag.Slots.OnListChanged += OnBagSlotChanged;
        }
        RefreshItems();
    }

    private void OnBagSlotChanged(NetworkListEvent<SlotData> e)
    {
        RefreshItems();
    }

    private void RefreshItems()
    {
        
    }

    private void OnDestroy()
    {
        if (m_MySelfPlayer != null && m_MySelfPlayer.Bag != null)
        {
            m_MySelfPlayer.Bag.Slots.OnListChanged -= OnBagSlotChanged;
        }
    }
}
