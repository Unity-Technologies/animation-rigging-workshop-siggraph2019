using System.Collections.Generic;
using Unity.Collections;

namespace UnityEngine.Animations.Rigging
{
    using Experimental.Animations;

    [Unity.Burst.BurstCompile]
    public struct TwistChainStep1Job : IWeightedAnimationJob
    {
        public ReadWriteTransformHandle rootTarget;
        public ReadWriteTransformHandle tipTarget;

        public NativeArray<ReadWriteTransformHandle> chain;

        public NativeArray<float> steps;

        public FloatProperty jobWeight { get; set; }

        public void ProcessRootMotion(AnimationStream stream) {}

        public void ProcessAnimation(AnimationStream stream)
        {
            // 1. Retrieve root and tip rotation.
            // q1 <- ROOT_TARGET_ROTATION
            // q2 <- TIP_TARGET_ROTATION

            // 2. Interpolate rotation on chain.
            // FOREACH(transform in chain)
            //     transform.rotation <- LERP(transform.rotation, LERP(q1, q2, w), jobWeight)

            // 3. Update position of tip handle for easier visualization.
            // ROOT_TARGET_POSITION <- ROOT_CHAIN_POSITION
            // TIP_TARGET_POSITION <- TIP_CHAIN_POSITION
        }
    }

    [System.Serializable]
    public struct TwistChainStep1Data : IAnimationJobData
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

    public class TwistChainStep1JobBinder : AnimationJobBinder<TwistChainStep1Job, TwistChainStep1Data>
    {
        public override TwistChainStep1Job Create(Animator animator, ref TwistChainStep1Data data, Component component)
        {
            // Retrieve chain in-between root and tip transforms.
            Transform[] chain = ConstraintsUtils.ExtractChain(data.root, data.tip);

            // Extract steps from chain.
            float[] steps = ConstraintsUtils.ExtractSteps(chain);

            // Build Job.
            var job = new TwistChainStep1Job();
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

        public override void Destroy(TwistChainStep1Job job)
        {
            job.chain.Dispose();
            job.steps.Dispose();
        }

        public override void Update(TwistChainStep1Job job, ref TwistChainStep1Data data)
        {
        }
    }

    [DisallowMultipleComponent, AddComponentMenu("SIGGRAPH 2019/Twist Chain Step 1")]
    public class TwistChainStep1 : RigConstraint<
        TwistChainStep1Job,
        TwistChainStep1Data,
        TwistChainStep1JobBinder
        >
    {
    }
}
