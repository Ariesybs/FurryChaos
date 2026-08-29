using KinematicCharacterController;
using UnityEngine;

public class CatFsmWalk : CatFsmBase
{
    public CatFsmWalk(CatCharacter cat) : base(cat)
    {
        
    }
    

    public override void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        base.UpdateVelocity(ref currentVelocity, deltaTime);
        var groundNormal = cat.motor.GroundingStatus.GroundNormal;
        // Vector3 direction = Vector3.ProjectOnPlane(simulatedForward, groundNormal).normalized;
    }

    public override void AfterCharacterUpdate(float deltaTime)
    {
        base.AfterCharacterUpdate(deltaTime);
    }
}