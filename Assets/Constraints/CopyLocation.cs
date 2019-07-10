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
    public float3 invert;

    public FloatProperty jobWeight { get; set; }

    public void ProcessRootMotion(AnimationStream stream) { }

    public void ProcessAnimation(AnimationStream stream)
    {
        float w = jobWeight.Get(stream);
        if (w > 0f)
        {
            constrained.SetPosition(
                stream,
                math.lerp(constrained.GetPosition(stream), source.GetPosition(stream) * invert, w)
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
            invert = new float3(data.xInvert ? -1f : 1f, data.yInvert ? -1f : 1f, data.zInvert ? -1f: 1f)
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
