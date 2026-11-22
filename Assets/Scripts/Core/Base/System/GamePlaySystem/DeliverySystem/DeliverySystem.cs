using System.Collections.Generic;
/// <summary>服务端事件：一批赃物被装上运输车。合约目标监听它来推进。</summary>
public struct MsgCargoLoaded : IMsg
{
    public ulong ClientId;
    public uint ItemId;
    public int Count;
    public int Weight; // 本批总重量(g)
}
public class DeliverySystem : SubSystem
{
    private readonly List<SlotData> m_SlotBuffer = new();
    private DeliveryPoint m_Point;
    public DeliverySystem(ISystem mainSystem) : base(mainSystem)
    {
        
    }
    private static InGameSession Session => GameRoot.Instance.GameFlow.InGameSystem.Session;
    public void RegisterDelivery(DeliveryPoint point)
    {
        m_Point = point;
    }
    public void UnregisterDelivery(DeliveryPoint point)
    {
        if (m_Point == point)
        {
            m_Point = null;
        }
    }
    /// <summary>把该玩家背包里的赃物全部装车，返回装车件数。只有自由状态的玩家可以装车。</summary>
    public int ServerLoadFromBag(ulong clientId)
    {
        var session = Session;
        if (session == null || !session.IsServer || session.Phase != MatchPhase.Playing ||
            session.GetStatus(clientId) != PlayerMatchStatus.Free)
        {
            return 0;
        }
        var player = PlayerMgr.Instance.GetPlayer(clientId);
        var bag = player != null ? player.Bag : null;
        if (bag == null)
        {
            return 0;
        }
        m_SlotBuffer.Clear();
        bag.ServerTakeLoot(m_SlotBuffer);
        var loadedCount = 0;
        for (var i = 0; i < m_SlotBuffer.Count; i++)
        {
            var slot = m_SlotBuffer[i];
            // 价值和重量一律从服务端配置表读取，不接受客户端传值
            var item = GameData.ItemDB.GetItemById(slot.ItemId);
            session.ServerAddCargo(slot.ItemId, slot.Count, item.LootValue * slot.Count);
            GameEvent.Trigger(new MsgCargoLoaded
            {
                ClientId = clientId,
                ItemId = slot.ItemId,
                Count = slot.Count,
                Weight = item.Weight * slot.Count
            });
            loadedCount += slot.Count;
        }
        return loadedCount;
    }
    /// <summary>自由 → 上车，已上车 → 下车。被关押和已离开的玩家不能操作。</summary>
    public void ServerToggleBoard(ulong clientId)
    {
        var session = Session;
        if (session == null || !session.IsServer || session.Phase != MatchPhase.Playing || m_Point == null ||
            !session.TryGetPlayerIndex(clientId, out var index))
        {
            return;
        }
        var cat = PlayerMgr.Instance.GetPlayer(clientId) as CatCharacter;
        if (cat == null)
        {
            return;
        }
        switch (session.Players[index].Status)
        {
            case PlayerMatchStatus.Free:
                ServerLoadFromBag(clientId); // 人上车时身上的赃物一并装车
                m_Point.GetSeatPose(index, out var seatPosition, out var seatRotation);
                cat.ServerSetRestrained(true, seatPosition, seatRotation);
                session.ServerSetStatus(clientId, PlayerMatchStatus.Aboard);
                break;
            case PlayerMatchStatus.Aboard:
                // 发车前允许反悔，比如下车去营救队友
                m_Point.GetExitPose(out var exitPosition, out var exitRotation);
                cat.ServerSetRestrained(false, exitPosition, exitRotation);
                session.ServerSetStatus(clientId, PlayerMatchStatus.Free);
                break;
        }
    }
}