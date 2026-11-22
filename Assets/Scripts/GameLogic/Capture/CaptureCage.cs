// Assets/Scripts/GameLogic/Capture/CaptureCage.cs
using UnityEngine;

/// <summary>关押被捕玩家的笼子。自由的玩家交互即可救出里面的队友。</summary>
public class CaptureCage : InteractableBase
{
    [SerializeField, Tooltip("笼内站位，被捕玩家依次分配")]
    private Transform[] m_HoldPoints;
    [SerializeField, Tooltip("被救出后的落脚点")]
    private Transform m_ReleasePoint;

    public override void OnNetworkSpawn()
    {
        GameRoot.Instance.GamePlay.CaptureSystem.RegisterCage(this);
    }

    public override void OnNetworkDespawn()
    {
        GameRoot.Instance.GamePlay.CaptureSystem.UnregisterCage(this);
    }

    /// <summary>被关押的玩家自己不能开笼。客户端用于提示，服务端用于校验。</summary>
    public override bool CanInteract(ulong netEntityId)
    {
        var session = GameRoot.Instance?.GameFlow?.InGameSystem?.Session;
        return session != null && session.GetStatus(netEntityId) == PlayerMatchStatus.Free;
    }

    protected override void ServerOnPressInteract(ulong clientId)
    {
        GameRoot.Instance.GamePlay.CaptureSystem.ServerRescue(clientId, this);
    }

    public void GetHoldPose(int index, out Vector3 position, out Quaternion rotation)
    {
        var hasPoints = m_HoldPoints != null && m_HoldPoints.Length > 0;
        var point = hasPoints ? m_HoldPoints[index % m_HoldPoints.Length] : transform;
        position = point.position;
        rotation = point.rotation;
    }

    public void GetReleasePose(out Vector3 position, out Quaternion rotation)
    {
        var point = m_ReleasePoint != null ? m_ReleasePoint : transform;
        position = point.position;
        rotation = point.rotation;
    }
}