using KinematicCharacterController;
using UnityEngine;

public class CatFsmBase : IFurryFSM
{
    protected CatCharacter cat;
    protected CatFSM.State CurState;
    protected CatFsmBase(CatCharacter cat)
    {
        this.cat = cat;
    }
    public virtual void OnInit()
    {
        
    }

    public virtual void OnEnter(CatFSM.State fromState)
    {
        
    }

    public virtual void OnUpdate()
    {
        
    }

    public virtual void OnInput(InputCmd cmd)
    {
        
    }

    public virtual void OnExit()
    {
        
    }

    public virtual void SwitchState(CatFSM.State fromState, CatFSM.State newState)
    {
        cat.catFsm.SwitchState(fromState,newState);
    }

    public virtual void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        
    }

    public virtual void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        
    }

    public virtual void BeforeCharacterUpdate(float deltaTime)
    {
        
    }

    public virtual void PostGroundingUpdate(float deltaTime)
    {
        
    }

    public virtual void AfterCharacterUpdate(float deltaTime)
    {
        
    }

    public virtual bool IsColliderValidForCollisions(Collider coll)
    {
        return true;
    }

    public virtual void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {
        
    }

    public virtual void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint,
        ref HitStabilityReport hitStabilityReport)
    {
        
    }

    public virtual void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition,
        Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
    {
        
    }

    public virtual void OnDiscreteCollisionDetected(Collider hitCollider)
    {
        
    }
}