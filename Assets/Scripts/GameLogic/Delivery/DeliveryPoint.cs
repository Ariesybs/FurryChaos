using UnityEngine;

public class DeliveryPoint : InteractableBase
{
    [SerializeField, Tooltip("车上的座位点，按玩家序号分配")]
    private Transform[] m_SeatPoints;
    [SerializeField, Tooltip("下车后的落脚点")]
    private Transform m_ExitPoint;
    
    public override string GetPrompt() => "离开";
    private void OnEnable()
    {
        GameRoot.Instance?.GamePlay?.DeliverySystem?.RegisterDelivery(this);
    }
    private void OnDisable()
    {
        GameRoot.Instance?.GamePlay?.DeliverySystem?.UnregisterDelivery(this);
    }
    public void GetSeatPose(int playerIndex, out Vector3 position, out Quaternion rotation)
    {
        var hasSeats = m_SeatPoints != null && m_SeatPoints.Length > 0;
        var seat = hasSeats ? m_SeatPoints[playerIndex % m_SeatPoints.Length] : transform;
        position = seat.position;
        rotation = seat.rotation;
    }
    public void GetExitPose(out Vector3 position, out Quaternion rotation)
    {
        var exit = m_ExitPoint != null ? m_ExitPoint : transform;
        position = exit.position;
        rotation = exit.rotation;
    }
    
    protected override void ServerOnPressInteract(ulong clientId)
    {
        GameRoot.Instance.GamePlay.DeliverySystem.ServerToggleBoard(clientId);
    }
}