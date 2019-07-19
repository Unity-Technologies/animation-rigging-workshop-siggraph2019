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

    public BoolProperty xInvert;
    public BoolProperty yInvert;
    public BoolProperty zInvert;

    public FloatProperty jobWeight { get; set; }

    public void ProcessRootMotion(AnimationStream stream) { }

    public void ProcessAnimation(AnimationStream stream)
    {
        float w = jobWeight.Get(stream);
        if (w > 0f)
        {
            float3 invertMask = new float3(
                math.select(1f, -1f, xInvert.Get(stream)),
                math.select(1f, -1f, yInvert.Get(stream)),
                math.select(1f, -1f, zInvert.Get(stream))
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
    [SyncSceneToStream] public bool xInvert;
    [SyncSceneToStream] public bool yInvert;
    [SyncSceneToStream] public bool zInvert;

    public bool IsValid()
    {
        return !(constrainedObject == null || sourceObject == null);
    }

    public void SetDefaultValues()
    {
        constrainedObject = null;
        sourceObject = null;
        xInvert = yInvert = zInvert = false;
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

            xInvert = BoolProperty.Bind(animator, component, PropertyUtils.ConstructConstraintDataPropertyName(nameof(data.xInvert))),
            yInvert = BoolProperty.Bind(animator, component, PropertyUtils.ConstructConstraintDataPropertyName(nameof(data.yInvert))),
            zInvert = BoolProperty.Bind(animator, component, PropertyUtils.ConstructConstraintDataPropertyName(nameof(data.zInvert)))
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
