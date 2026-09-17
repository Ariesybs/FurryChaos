// 管理实体、输入历史、快照

using System.Collections.Generic;
using UnityEngine;

public class CatSyncSystem : LogicSystem
{
    private readonly Dictionary<long, CatNetworkEntity> m_Cats = new();
    private uint m_ServerTick;
    private CatCharacter.CatAnimationState m_AnimationState;
    public void RegisterCat(CatNetworkEntity entity)
    {
        if (entity == null)
        {
            return;
        }

        m_Cats.TryAdd(entity.EntityId, entity);
    }

    public void UnregisterCat(CatNetworkEntity entity)
    {
        if (entity == null)
        {
            return;
        }

        m_Cats.Remove(entity.EntityId);
    }

    public void SubmitLocalInput(long entityId,InputCmd cmd)
    {
        if (!m_Cats.TryGetValue(entityId, out CatNetworkEntity entity))
        {
            return;
        }

        SendInput(cmd);
        // entity.ApplyInput(cmd);
    }

    public override void OnFixedUpdate(float deltaTime)
    {
        base.OnFixedUpdate(deltaTime);
        
#if UNITY_SERVER
        m_ServerTick++;
        foreach (var pair in m_Cats)
        {
            CatNetworkEntity entity = pair.Value;
            if (entity == null ||entity.Role != CatNetworkRole.ServerAuthority)
            {
                continue;
            }
            SendSnapshot(entity);
        }
#endif
    }

    private void SendInput(InputCmd cmd)
    {
        var msg = NetworkMsg.Get<C2S_CatInputRequest>();
        msg.SetFromCmd(cmd);
        GameNet.SendS(msg);
        msg.Release();
    } 
    private void SendSnapshot(CatNetworkEntity entity)
    {
        var character = entity.GetComponent<CatCharacter>();
        if (character == null || character.motor == null)
        {
            return;
        }
        var msg = NetworkMsg.Get<S2C_CatSnapshot>();
        msg.EntityId = entity.EntityId;
        msg.ServerTick = m_ServerTick;
        msg.LastProcessedInputSequence = 0;
        msg.Position = character.motor.TransientPosition;
        msg.Rotation = character.motor.TransientRotation;
        msg.Velocity = character.motor.Velocity;
        msg.MovementState = (byte)character.CurrentAnimationState;;
        msg.IsGrounded = character.motor.GroundingStatus.IsStableOnGround;
        GameNet.Broadcast(msg);
        NetworkMsg.Release(msg);
    }

    //服务端收到客户端输入后调用。
    public void HandleClientInput(long entityId, InputCmd cmd)
    {
        if (!m_Cats.TryGetValue(entityId, out CatNetworkEntity entity))
        {
            return;
        }
        if (entity.Role != CatNetworkRole.ServerAuthority)
        {
            return;
        }
        entity.ApplyInput(cmd);
    }

    // 客户端收到位置快照后调用。
    public void HandleSnapshot(S2C_CatSnapshot msg)
    {
        if (!m_Cats.TryGetValue(msg.EntityId, out CatNetworkEntity entity))
        {
            return;
        }
        entity.ApplySnapshot(
            msg.Position,
            msg.Rotation,
            msg.Velocity,
            (CatCharacter.CatAnimationState)msg.MovementState);
    }
}