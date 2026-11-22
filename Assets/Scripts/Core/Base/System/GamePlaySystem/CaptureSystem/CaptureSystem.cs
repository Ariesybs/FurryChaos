using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 被捕与营救（仅服务端生效）。被捕不是出局：
/// 猫被关进最近的笼子，自由的队友可以交互救出；发车时仍被关押才计赎金。
/// </summary>
public class CaptureSystem : SubSystem
{
    private const string Tag = "CaptureSystem";
    private readonly List<CaptureCage> m_Cages = new();
    private readonly Dictionary<ulong, CaptureCage> m_Held = new();
    private readonly List<ulong> m_ReleaseBuffer = new();

    public CaptureSystem(ISystem mainSystem) : base(mainSystem)
    {
    }

    private static InGameSession Session => GameRoot.Instance.GameFlow.InGameSystem.Session;

    public void RegisterCage(CaptureCage cage)
    {
        if (cage != null && !m_Cages.Contains(cage))
        {
            m_Cages.Add(cage);
        }
    }

    public void UnregisterCage(CaptureCage cage)
    {
        m_Cages.Remove(cage);
    }

    /// <summary>
    /// 只有自由状态的玩家会被捕，所以狗的重复命中、对已上车玩家的命中都会被拒绝。
    /// </summary>
    public bool ServerCapture(ulong clientId)
    {
        var session = Session;
        if (session == null || !session.IsServer || session.Phase != MatchPhase.Playing ||
            session.GetStatus(clientId) != PlayerMatchStatus.Free)
        {
            return false;
        }

        var cat = PlayerMgr.Instance.GetPlayer(clientId) as CatCharacter;
        if (cat == null)
        {
            return false;
        }

        var cage = FindNearestCage(cat.transform.position);
        if (cage == null)
        {
            Log.Error(Tag, "场景中没有 CaptureCage，无法关押玩家");
            return false;
        }

        cage.GetHoldPose(m_Held.Count, out var position, out var rotation);
        cat.ServerSetRestrained(true, position, rotation);
        m_Held[clientId] = cage;
        session.ServerSetStatus(clientId, PlayerMatchStatus.Captured);
        return true;
    }

    /// <summary>营救者必须是自由状态；一次救出该笼子里的所有玩家。返回救出人数。</summary>
    public int ServerRescue(ulong rescuerId, CaptureCage cage)
    {
        var session = Session;
        if (session == null || !session.IsServer || session.Phase != MatchPhase.Playing ||
            session.GetStatus(rescuerId) != PlayerMatchStatus.Free)
        {
            return 0;
        }

        m_ReleaseBuffer.Clear();
        foreach (var pair in m_Held)
        {
            if (pair.Value == cage)
            {
                m_ReleaseBuffer.Add(pair.Key);
            }
        }

        cage.GetReleasePose(out var position, out var rotation);
        for (var i = 0; i < m_ReleaseBuffer.Count; i++)
        {
            var clientId = m_ReleaseBuffer[i];
            m_Held.Remove(clientId);
            var cat = PlayerMgr.Instance.GetPlayer(clientId) as CatCharacter;
            if (cat != null)
            {
                cat.ServerSetRestrained(false, position, rotation);
            }

            session.ServerSetStatus(clientId, PlayerMatchStatus.Free);
        }

        return m_ReleaseBuffer.Count;
    }

    /// <summary>玩家掉线时清掉关押记录。</summary>
    public void ServerForget(ulong clientId)
    {
        m_Held.Remove(clientId);
    }

    public void ServerReset()
    {
        m_Held.Clear();
    }

    private CaptureCage FindNearestCage(Vector3 position)
    {
        CaptureCage nearest = null;
        var bestSqrDistance = float.PositiveInfinity;
        for (var i = 0; i < m_Cages.Count; i++)
        {
            var sqrDistance = (m_Cages[i].transform.position - position).sqrMagnitude;
            if (sqrDistance < bestSqrDistance)
            {
                bestSqrDistance = sqrDistance;
                nearest = m_Cages[i];
            }
        }

        return nearest;
    }

    public override void OnDispose()
    {
        m_Cages.Clear();
        m_Held.Clear();
    }
}