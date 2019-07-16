using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Experimental.Animations;
using Unity.Burst;
using Unity.Mathematics;

[BurstCompile]
public struct CopyLocationStep1Job : IWeightedAnimationJob
{
    public ReadWriteTransformHandle constrained;
    public ReadOnlyTransformHandle  source;

    // TODO : Update mask so it is dynamic
    public float3 invertMask;

    public FloatProperty jobWeight { get; set; }

    public void ProcessRootMotion(AnimationStream stream) { }

    public void ProcessAnimation(AnimationStream stream)
    {
        float w = jobWeight.Get(stream);
        if (w > 0f)
        {
            constrained.SetPosition(
                stream,
                math.lerp(constrained.GetPosition(stream), source.GetPosition(stream) * invertMask, w)
                );
        }
    }
}

[System.Serializable]
public struct CopyLocationStep1Data : IAnimationJobData
{
    public Transform constrainedObject;
    [SyncSceneToStream] public Transform sourceObject;

    // TODO : Update fields to make these dynamic
    public bool xInvert;
    public bool yInvert;
    public bool zInvert;

    public bool IsValid()
    {
        return !(constrainedObject == null || sourceObject == null);
    }

    public void SetDefaultValues()
    {
        constrainedObject = null;
        sourceObject = null;
        xInvert = yInvert = zInvert = true;
    }
}

public class CopyLocationStep1Binder : AnimationJobBinder<CopyLocationStep1Job, CopyLocationStep1Data>
{
    public override CopyLocationStep1Job Create(Animator animator, ref CopyLocationStep1Data data, Component component)
    {
        return new CopyLocationStep1Job()
        {
            constrained = ReadWriteTransformHandle.Bind(animator, data.constrainedObject),
            source = ReadOnlyTransformHandle.Bind(animator, data.sourceObject),

            // TODO : Bind data to stream so it becomes dynamic
            invertMask = new float3(data.xInvert ? -1f : 1f, data.yInvert ? -1f : 1f, data.zInvert ? -1f : 1f)
        };
    }

    public override void Destroy(CopyLocationStep1Job job) { }
}

[DisallowMultipleComponent, AddComponentMenu("SIGGRAPH 2019/Copy Location (Step 1)")]
public class CopyLocationStep1 : RigConstraint<
    CopyLocationStep1Job,
    CopyLocationStep1Data,
    CopyLocationStep1Binder
    >
{
}
