using System.Collections.Generic;
using UnityEditor;
using Unity.Collections;

namespace UnityEngine.Animations.Rigging
{
    using Experimental.Animations;

    [Unity.Burst.BurstCompile]
    public struct TwistChainStep0Job : IWeightedAnimationJob
    {
        public FloatProperty jobWeight { get; set; }

        public void ProcessRootMotion(AnimationStream stream) {}

        public void ProcessAnimation(AnimationStream stream) {}
    }

    [System.Serializable]
    public struct TwistChainStep0Data : IAnimationJobData
    {
        bool IAnimationJobData.IsValid() => true;

        void IAnimationJobData.SetDefaultValues()
        {
        }
    }

    public class TwistChainStep0JobBinder : AnimationJobBinder<TwistChainStep0Job, TwistChainStep0Data>
    {
        public override TwistChainStep0Job Create(Animator animator, ref TwistChainStep0Data data, Component component)
        {
            // Build Job.
            var job = new TwistChainStep0Job();
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
