using System.Collections.Generic;
using Unity.Collections;

namespace UnityEngine.Animations.Rigging
{
    using Experimental.Animations;

    [Unity.Burst.BurstCompile]
    public struct TwistChainJob : IWeightedAnimationJob
    {
        public ReadWriteTransformHandle rootTarget;
        public ReadWriteTransformHandle tipTarget;

        public NativeArray<ReadWriteTransformHandle> chain;

        public NativeArray<float> steps;
        public NativeArray<float> weights;

        public FloatProperty jobWeight { get; set; }

        public void ProcessRootMotion(AnimationStream stream) { }

        public void ProcessAnimation(AnimationStream stream)
        {
            // Retrieve root and tip rotation.
            Quaternion rootRotation = rootTarget.GetRotation(stream);
            Quaternion tipRotation = tipTarget.GetRotation(stream);

            float mainWeight = jobWeight.Get(stream);

            // Interpolate rotation on chain.
            for (int i = 0; i < chain.Length; ++i)
            {
                chain[i].SetRotation(stream, Quaternion.Lerp(chain[i].GetRotation(stream), Quaternion.Lerp(rootRotation, tipRotation, weights[i]), mainWeight));
            }

            // Update position of tip handle for easier visualization.
            rootTarget.SetPosition(stream, chain[0].GetPosition(stream));
            tipTarget.SetPosition(stream, chain[chain.Length - 1].GetPosition(stream));
        }
    }

    [System.Serializable]
    public struct TwistChainData : IAnimationJobData
    {
        public Transform root;
        public Transform tip;

        [SyncSceneToStream] public Transform rootTarget;
        [SyncSceneToStream] public Transform tipTarget;

        public AnimationCurve curve;

        bool IAnimationJobData.IsValid() => !(root == null || tip == null || !tip.IsChildOf(root) || rootTarget == null || tipTarget == null || curve == null);

        void IAnimationJobData.SetDefaultValues()
        {
            root = tip = rootTarget = tipTarget = null;
            curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        }
    }


    public class TwistChainJobBinder : AnimationJobBinder<TwistChainJob, TwistChainData>
    {
        public override TwistChainJob Create(Animator animator, ref TwistChainData data, Component component)
        {
            // Retrieve chain in-between root and tip transforms.
            Transform[] chain = ConstraintsUtils.ExtractChain(data.root, data.tip);

            // Extract steps from chain.
            float[] steps = ConstraintsUtils.ExtractSteps(chain);

            // Build Job.
            var job = new TwistChainJob();
            job.chain = new NativeArray<ReadWriteTransformHandle>(chain.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            job.steps = new NativeArray<float>(chain.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            job.weights = new NativeArray<float>(chain.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            job.rootTarget = ReadWriteTransformHandle.Bind(animator, data.rootTarget);
            job.tipTarget = ReadWriteTransformHandle.Bind(animator, data.tipTarget);

            // Set values in NativeArray.
            for (int i = 0; i < chain.Length; ++i)
            {
                job.chain[i] = ReadWriteTransformHandle.Bind(animator, chain[i]);
                job.steps[i] = steps[i];
                job.weights[i] = Mathf.Clamp01(data.curve.Evaluate(steps[i]));
            }

            return job;
        }

        public override void Destroy(TwistChainJob job)
        {
            job.chain.Dispose();
            job.weights.Dispose();
            job.steps.Dispose();
        }

        public override void Update(TwistChainJob job, ref TwistChainData data)
        {
            // Update weights based on curve.
            for (int i = 0; i < job.steps.Length; ++i)
            {
                job.weights[i] = Mathf.Clamp01(data.curve.Evaluate(job.steps[i]));
            }
        }
    }

    [DisallowMultipleComponent, AddComponentMenu("SIGGRAPH 2019/Twist Chain Final")]
    public class TwistChain : RigConstraint<
        TwistChainJob,
        TwistChainData,
        TwistChainJobBinder
        >
    {
    }
}
