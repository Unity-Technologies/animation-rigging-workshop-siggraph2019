using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Experimental.Animations;
using Unity.Burst;
using Unity.Mathematics;

[BurstCompile]
public struct CopyLocationJob : IWeightedAnimationJob
{
    public ReadWriteTransformHandle constrained;
    public ReadOnlyTransformHandle  source;

    // Use Vector3BoolProperty to read in the axis booleans from
    // the AnimationStream
    public Vector3BoolProperty invert;

    public FloatProperty jobWeight { get; set; }

    public void ProcessRootMotion(AnimationStream stream) { }

    public void ProcessAnimation(AnimationStream stream)
    {
        float w = jobWeight.Get(stream);
        if (w > 0f)
        {
            var tmp = invert.Get(stream);
            float3 invertMask = new float3(
                math.select(1f, -1f, tmp.x),
                math.select(1f, -1f, tmp.y),
                math.select(1f, -1f, tmp.z)
                );

            constrained.SetPosition(
                stream,
                math.lerp(constrained.GetPosition(stream), source.GetPosition(stream) * invertMask, w)
                );
        }
    }
}

[System.Serializable]
public struct CopyLocationData : IAnimationJobData
{
    public Transform constrainedObject;
    [SyncSceneToStream] public Transform sourceObject;

    // Use SyncSceneToStream on Vector3Bool in order to add these
    // properties to the AnimationStream and for them to be
    // updated by the intial SyncSceneToStream job defined by the RigBuilder.
    [SyncSceneToStream] public Vector3Bool invert;

    public bool IsValid()
    {
        return !(constrainedObject == null || sourceObject == null);
    }

    public void SetDefaultValues()
    {
        constrainedObject = null;
        sourceObject = null;
        invert = new Vector3Bool(false);
    }
}

public class CopyLocationBinder : AnimationJobBinder<CopyLocationJob, CopyLocationData>
{
    public override CopyLocationJob Create(Animator animator, ref CopyLocationData data, Component component)
    {
        return new CopyLocationJob()
        {
            constrained = ReadWriteTransformHandle.Bind(animator, data.constrainedObject),
            source = ReadOnlyTransformHandle.Bind(animator, data.sourceObject),

            // Bind data.invert to job.invert so values can be resolved from the AnimationStream
            invert = Vector3BoolProperty.Bind(animator, component, PropertyUtils.ConstructConstraintDataPropertyName(nameof(data.invert)))
        };
    }

    public override void Destroy(CopyLocationJob job) { }
}

[DisallowMultipleComponent, AddComponentMenu("SIGGRAPH 2019/Copy Location")]
public class CopyLocation : RigConstraint<
    CopyLocationJob,
    CopyLocationData,
    CopyLocationBinder
    >
{
}
