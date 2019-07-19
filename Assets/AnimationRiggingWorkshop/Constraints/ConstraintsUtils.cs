using System;
using System.Collections.Generic;

namespace UnityEngine.Animations.Rigging
{
    static class ConstraintsUtils
    {
        public static Transform[] ExtractChain(Transform root, Transform tip)
        {
            if (!tip.IsChildOf(root))
                return new Transform[0]{};

            var chain = new List<Transform>();

            Transform tmp = tip;
            while (tmp != root)
            {
                chain.Add(tmp);
                tmp = tmp.parent;
            }
            chain.Add(root);
            chain.Reverse();

            return chain.ToArray();
        }

        public static float[] ExtractLengths(Transform[] chain)
        {
            float[] lengths = new float[chain.Length];
            lengths[0] = 0f;

            // Evaluate lengths as distance between each transform in the chain.
            for (int i = 1; i < chain.Length; ++i)
            {
                lengths[i] = chain[i].localPosition.magnitude;
            }

            return lengths;
        }

        public static float[] ExtractSteps(Transform[] chain)
        {
            float[] lengths = ExtractLengths(chain);

            float totalLength = 0f;
            Array.ForEach(lengths, (length) => totalLength += length);

            float[] steps = new float[lengths.Length];

            // Evaluate weights and steps based on curve.
            float cumulativeLength = 0.0f;
            for (int i = 0; i < lengths.Length; ++i)
            {
                cumulativeLength += lengths[i];

                float t = cumulativeLength / totalLength;

                steps[i] = t;
            }

            return steps;
        }
    }
}
