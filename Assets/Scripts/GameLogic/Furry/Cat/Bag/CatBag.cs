using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CatBag : NetworkBehaviour
{
    [SerializeField, Min(1)] private int baseMaxWeight = 10;
    public NetworkVariable<int> MaxWeight;
    public NetworkVariable<int> UsedWeight = new();
    public int FreeWeight => Mathf.Max(0, MaxWeight.Value - UsedWeight.Value);
    public bool IsFull => FreeWeight <= 0;
    
    private readonly NetworkList<BagItemEntity> m_ItemList = new();
    public NetworkList<BagItemEntity> Items => m_ItemList;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        MaxWeight = new NetworkVariable<int>(baseMaxWeight);
        m_ItemList.OnListChanged += OnItemListChanged;
    }

    private void OnItemListChanged(NetworkListEvent<BagItemEntity> changeEvent)
    {
        // 重新计算背包容量
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddItemToBagServerRpc(BagItemEntity item)
    {
        if (IsFull)
        {
            return;
        }
        
    }

    [ClientRpc]
    public void AddItemToBagClientRpc(BagItemEntity item)
    {
        
    }
}
