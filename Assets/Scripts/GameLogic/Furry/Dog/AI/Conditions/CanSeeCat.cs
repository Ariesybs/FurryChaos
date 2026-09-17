using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("Dog/Perception")]
[TaskDescription("寻找一只当前可见的猫，并写入 Target。")]
public sealed class CanSeeCat : Conditional
{
    [SharedRequired]
    public SharedGameObject Target;
    
    private DogVision vision;

    public override void OnAwake()
    {
        vision = GetComponent<DogVision>();
    }

    public override TaskStatus OnUpdate()
    {
        if (vision == null)
        {
            // 无视觉组件
            return TaskStatus.Failure;
        }
        // 优先保持当前目标，避免在多只猫之间频繁跳动。
        if (Target.Value != null && vision.CanSeeCat(Target.Value))
        {
            return TaskStatus.Success;
        }

        if (vision.TryFindVisibleCat(out GameObject cat))
        {
            Target.Value = cat;
            return TaskStatus.Success;
        }
        return TaskStatus.Failure;
    }
    
    public override void OnReset()
    {
        Target = null;
    }
}
