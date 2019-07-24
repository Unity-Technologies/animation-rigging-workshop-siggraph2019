using System.Collections.Generic;
using Unity.Collections;

namespace UnityEngine.Animations.Rigging
{
    using Experimental.Animations;

    [Unity.Burst.BurstCompile]
    public struct TwistChainStep0Job : IWeightedAnimationJob
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
    public struct TwistChainStep0Data : IAnimationJobData
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

    public class TwistChainStep0JobBinder : AnimationJobBinder<TwistChainStep0Job, TwistChainStep0Data>
    {
        public override TwistChainStep0Job Create(Animator animator, ref TwistChainStep0Data data, Component component)
        {
            // 1. Retrieve chain in-between root and tip transforms.
            // ...

            // 2. Extract steps from chain.
            // ...

            // 3. Build Job.
            var job = new TwistChainStep0Job();
            // ...

            // 4. Set values in NativeArray.
            // ...

            return job;
        }

        public override void Destroy(TwistChainStep0Job job)
        {
        }

        public override void Update(TwistChainStep0Job job, ref TwistChainStep0Data data)
        {
        }
    }

    [DisallowMultipleComponent, AddComponentMenu("SIGGRAPH 2019/Twist Chain Step 0")]
    public class TwistChainStep0 : RigConstraint<
        TwistChainStep0Job,
        TwistChainStep0Data,
        TwistChainStep0JobBinder
        >
    {
    }
}
