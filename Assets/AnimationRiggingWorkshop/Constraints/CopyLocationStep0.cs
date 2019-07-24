using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Experimental.Animations;
using Unity.Burst;
using Unity.Mathematics;

[BurstCompile]
public struct CopyLocationStep0Job : IWeightedAnimationJob
{
    public ReadWriteTransformHandle constrained;
    public ReadOnlyTransformHandle  source;

    // TODO : Add invert axis mask

    public FloatProperty jobWeight { get; set; }

    public void ProcessRootMotion(AnimationStream stream) { }

    public void ProcessAnimation(AnimationStream stream)
    {
        float w = jobWeight.Get(stream);
        if (w > 0f)
        {
            // TODO : Change code to consider inverted axis positions instead

            constrained.SetPosition(
                stream,
                math.lerp(constrained.GetPosition(stream), -source.GetPosition(stream), w)
                );
        }
    }
}

[System.Serializable]
public struct CopyLocationStep0Data : IAnimationJobData
{
    public Transform constrainedObject;
    [SyncSceneToStream] public Transform sourceObject;

    // TODO : Add invert axis booleans

    public bool IsValid()
    {
        return !(constrainedObject == null || sourceObject == null);
    }

    public void SetDefaultValues()
    {
        constrainedObject = null;
        sourceObject = null;

        // TODO : Set initial values to invert booleans
    }
}

public class CopyLocationStep0Binder : AnimationJobBinder<CopyLocationStep0Job, CopyLocationStep0Data>
{
    public override CopyLocationStep0Job Create(Animator animator, ref CopyLocationStep0Data data, Component component)
    {
        return new CopyLocationStep0Job()
        {
            constrained = ReadWriteTransformHandle.Bind(animator, data.constrainedObject),
            source = ReadOnlyTransformHandle.Bind(animator, data.sourceObject)

            // TODO : Update binder code to add our new toggles
        };
    }

    public override void Destroy(CopyLocationStep0Job job) { }
}

[DisallowMultipleComponent, AddComponentMenu("SIGGRAPH 2019/Copy Location (Step 0)")]
public class CopyLocationStep0 : RigConstraint<
    CopyLocationStep0Job,
    CopyLocationStep0Data,
    CopyLocationStep0Binder
    >
{
}
