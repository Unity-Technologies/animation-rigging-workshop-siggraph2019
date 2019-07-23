using System.Collections.Generic;
using Unity.Collections;

namespace UnityEngine.Animations.Rigging
{
    using Experimental.Animations;

    [Unity.Burst.BurstCompile]
    public struct TwistChainStep2Job : IWeightedAnimationJob
    {
        public ReadWriteTransformHandle rootTarget;
        public ReadWriteTransformHandle tipTarget;

        public NativeArray<ReadWriteTransformHandle> chain;

        public NativeArray<float> steps;

        public FloatProperty jobWeight { get; set; }

        public void ProcessRootMotion(AnimationStream stream) {}

        public void ProcessAnimation(AnimationStream stream)
        {
            // Retrieve root and tip rotation.
            Quaternion rootRotation = rootTarget.GetRotation(stream);
            Quaternion tipRotation = tipTarget.GetRotation(stream);

            float mainWeight = jobWeight.Get(stream);

            // Interpolate rotation on chain.
            for (int i = 0; i < chain.Length; ++i)
            {
                chain[i].SetRotation(stream, Quaternion.Lerp(chain[i].GetRotation(stream), Quaternion.Lerp(rootRotation, tipRotation, steps[i]), mainWeight));
            }

            // Update position of tip handle for easier visualization.
            rootTarget.SetPosition(stream, chain[0].GetPosition(stream));
            tipTarget.SetPosition(stream, chain[chain.Length - 1].GetPosition(stream));
        }
    }

    [System.Serializable]
    public struct TwistChainStep2Data : IAnimationJobData
    {
        public Transform root;
        public Transform tip;

        [SyncSceneToStream] public Transform rootTarget;
        [SyncSceneToStream] public Transform tipTarget;

        bool IAnimationJobData.IsValid() => !(root == null || tip == null || !tip.IsChildOf(root) || rootTarget == null || tipTarget == null);

        void IAnimationJobData.SetDefaultValues()
        {
            root = tip = rootTarget = tipTarget = null;
        }
    }

    public class TwistChainStep2JobBinder : AnimationJobBinder<TwistChainStep2Job, TwistChainStep2Data>
    {
        public override TwistChainStep2Job Create(Animator animator, ref TwistChainStep2Data data, Component component)
        {
            // Retrieve chain in-between root and tip transforms.
            Transform[] chain = ConstraintsUtils.ExtractChain(data.root, data.tip);

            // Extract steps from chain.
            float[] steps = ConstraintsUtils.ExtractSteps(chain);

            // Build Job.
            var job = new TwistChainStep2Job();
            job.chain = new NativeArray<ReadWriteTransformHandle>(chain.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            job.steps = new NativeArray<float>(chain.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            job.rootTarget = ReadWriteTransformHandle.Bind(animator, data.rootTarget);
            job.tipTarget = ReadWriteTransformHandle.Bind(animator, data.tipTarget);

            // Set values in NativeArray.
            for (int i = 0; i < chain.Length; ++i)
            {
                job.chain[i] = ReadWriteTransformHandle.Bind(animator, chain[i]);
                job.steps[i] = steps[i];
            }

            return job;
        }

        public override void Destroy(TwistChainStep2Job job)
        {
            job.chain.Dispose();
            job.steps.Dispose();
        }

        public override void Update(TwistChainStep2Job job, ref TwistChainStep2Data data)
        {
        }
    }

    [DisallowMultipleComponent, AddComponentMenu("SIGGRAPH 2019/Twist Chain Step 2")]
    public class TwistChainStep2 : RigConstraint<
        TwistChainStep2Job,
        TwistChainStep2Data,
        TwistChainStep2JobBinder
        >
    {
    }
}
