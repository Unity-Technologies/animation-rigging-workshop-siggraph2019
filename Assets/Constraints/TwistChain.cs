using System.Collections.Generic;
using Unity.Collections;

namespace UnityEngine.Animations.Rigging
{
    using Experimental.Animations;

    [Unity.Burst.BurstCompile]
    public struct TwistChainJob : IWeightedAnimationJob
    {
        public NativeArray<ReadWriteTransformHandle> chain;
        public NativeArray<float> steps;
        public NativeArray<float> weights;

        public ReadWriteTransformHandle rootTarget;
        public ReadWriteTransformHandle tipTarget;

        public FloatProperty jobWeight { get; set; }

        public void ProcessRootMotion(AnimationStream stream) { }

        public void ProcessAnimation(AnimationStream stream)
        {
            float w = jobWeight.Get(stream);
            if (w > 0f)
            {
                // Retrieve root and tip rotation.
                Quaternion rootRotation = rootTarget.GetRotation(stream);
                Quaternion tipRotation = tipTarget.GetRotation(stream);

                // Interpolate rotation on chain.
                for (int i = 0; i < chain.Length; ++i)
                {
                    chain[i].SetRotation(stream, Quaternion.Lerp(chain[i].GetRotation(stream), Quaternion.Lerp(rootRotation, tipRotation, weights[i]), w));
                }

                // Update position of handles for easier
                rootTarget.SetPosition(stream, chain[0].GetPosition(stream));
                tipTarget.SetPosition(stream, chain[chain.Length - 1].GetPosition(stream));
            }
            else
            {
                for (int i = 0; i < chain.Length; ++i)
                    AnimationRuntimeUtils.PassThrough(stream, chain[i]);
            }
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
            // Default values.
            root = null;
            tip = null;

            rootTarget = null;
            tipTarget = null;

            curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        }
    }


    public class TwistChainJobBinder : AnimationJobBinder<TwistChainJob, TwistChainData>
    {
        public override TwistChainJob Create(Animator animator, ref TwistChainData data, Component component)
        {
            // Retrieve chain in-between root and tip transforms.
            List<Transform> chain = new List<Transform>();
            Transform tmp = data.tip;
            while (tmp != data.root)
            {
                chain.Add(tmp);
                tmp = tmp.parent;
            }
            chain.Add(data.root);
            chain.Reverse();

            // Build Job.
            var job = new TwistChainJob();
            job.chain = new NativeArray<ReadWriteTransformHandle>(chain.Count, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            job.steps = new NativeArray<float>(chain.Count, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            job.weights = new NativeArray<float>(chain.Count, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            job.rootTarget = ReadWriteTransformHandle.Bind(animator, data.rootTarget);
            job.tipTarget = ReadWriteTransformHandle.Bind(animator, data.tipTarget);

            for (int i = 0; i < chain.Count; ++i)
            {
                job.chain[i] = ReadWriteTransformHandle.Bind(animator, chain[i]);
            }

            // Extract lengths from chain.
            float totalLength = 0f;

            float[] lengths = new float[chain.Count];
            lengths[0] = 0f;

            for (int i = 1; i < chain.Count; ++i)
            {
                lengths[i] = chain[i].localPosition.magnitude;
                totalLength += lengths[i];
            }

            // Evaluate weights and steps based on curve.
            float cumulativeLength = 0.0f;
            for (int i = 0; i < lengths.Length; ++i)
            {
                cumulativeLength += lengths[i];

                float t = cumulativeLength / totalLength;

                job.steps[i] = t;
                job.weights[i] = Mathf.Clamp01(data.curve.Evaluate(t));
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
            for (int i = 0; i < job.weights.Length; ++i)
            {
                job.weights[i] = Mathf.Clamp01(data.curve.Evaluate(job.steps[i]));
            }
        }
    }

    [DisallowMultipleComponent, AddComponentMenu("SIGGRAPH 2019/Twist Chain")]
    public class TwistChain : RigConstraint<
        TwistChainJob,
        TwistChainData,
        TwistChainJobBinder
        >
    {
    }
}
