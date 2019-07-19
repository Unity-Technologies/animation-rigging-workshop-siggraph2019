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
                float weight = Mathf.InverseLerp(0, chain.Length - 1, i);
                chain[i].SetRotation(stream, Quaternion.Lerp(chain[i].GetRotation(stream), Quaternion.Lerp(rootRotation, tipRotation, weight), mainWeight));
            }

            // Update position of tip handle for easier visualization.
            rootTarget.SetPosition(stream, chain[0].GetPosition(stream));
            tipTarget.SetPosition(stream, chain[chain.Length - 1].GetPosition(stream));
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

            // Build Job.
            var job = new TwistChainStep1Job();
            job.chain = new NativeArray<ReadWriteTransformHandle>(chain.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            job.rootTarget = ReadWriteTransformHandle.Bind(animator, data.rootTarget);
            job.tipTarget = ReadWriteTransformHandle.Bind(animator, data.tipTarget);

            for (int i = 0; i < chain.Length; ++i)
                job.chain[i] = ReadWriteTransformHandle.Bind(animator, chain[i]);

            return job;
        }

        public override void Destroy(TwistChainStep1Job job)
        {
            job.chain.Dispose();
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
