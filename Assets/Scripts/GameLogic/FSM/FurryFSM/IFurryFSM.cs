using KinematicCharacterController;
using UnityEngine;

public interface IFurryFSM
{
    public void OnInit();
    public void OnEnter(CatFSM.State fromState);
    public void OnUpdate();
    public void OnInput(InputCmd cmd);
    public void OnExit();
    
    #region KCC相关
    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime);
    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime) ;
    public void BeforeCharacterUpdate(float deltaTime);
    public void PostGroundingUpdate(float deltaTime);
    public void AfterCharacterUpdate(float deltaTime);
    public bool IsColliderValidForCollisions(Collider coll);
    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport);
    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint,
        ref HitStabilityReport hitStabilityReport);
    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint,
        Vector3 atCharacterPosition,
        Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport);
    public void OnDiscreteCollisionDetected(Collider hitCollider);

    #endregion
    
}