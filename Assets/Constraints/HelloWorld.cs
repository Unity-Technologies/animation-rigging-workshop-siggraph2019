using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Experimental.Animations;
using Unity.Burst;
using Unity.Mathematics;

[BurstCompile]
public struct HelloWorldJob : IWeightedAnimationJob
{
    public ReadWriteTransformHandle constrained;
    public ReadOnlyTransformHandle  source;
    
    public FloatProperty jobWeight { get; set; }

    public void ProcessRootMotion(AnimationStream stream) { }

    public void ProcessAnimation(AnimationStream stream)
    {
        float w = jobWeight.Get(stream);
        if (w > 0f)
        {
            constrained.SetPosition(
                stream,
                math.lerp(constrained.GetPosition(stream), -source.GetPosition(stream), w)
                );
        }
    }
}

[System.Serializable]
public struct HelloWorldData : IAnimationJobData
{
    public Transform constrainedObject;
    [SyncSceneToStream] public Transform sourceObject;

    public bool IsValid()
    {
        return !(constrainedObject == null || sourceObject == null);
    }

    public void SetDefaultValues()
    {
        constrainedObject = null;
        sourceObject = null;
    }
}

public class HelloWorldBinder : AnimationJobBinder<HelloWorldJob, HelloWorldData>
{
    public override HelloWorldJob Create(Animator animator, ref HelloWorldData data, Component component)
    {
        return new HelloWorldJob()
        {
            constrained = ReadWriteTransformHandle.Bind(animator, data.constrainedObject),
            source = ReadOnlyTransformHandle.Bind(animator, data.sourceObject)
        };
    }

    public override void Destroy(HelloWorldJob job) { }
}

[DisallowMultipleComponent, AddComponentMenu("SIGGRAPH 2019/Hello World")]
public class HelloWorld : RigConstraint<
    HelloWorldJob,
    HelloWorldData,
    HelloWorldBinder
    >
{
}
