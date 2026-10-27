using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(NetworkObject))]
public class Item : NetworkBehaviour
{
    [FormerlySerializedAs("Config")] public ItemData data;
}