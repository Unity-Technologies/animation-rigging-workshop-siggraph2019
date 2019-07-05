using System.Collections.Generic;
using UnityEditor;
using Unity.Collections;

namespace UnityEngine.Animations.Rigging
{
    static class ConstraintsUtils
    {
        public static List<Transform> ExtractChain(Transform root, Transform tip)
        {
            var chain = new List<Transform>();

            if (!tip.IsChildOf(root))
                return chain;

            Transform tmp = tip;
            while (tmp != root)
            {
                chain.Add(tmp);
                tmp = tmp.parent;
            }
            chain.Add(root);
            chain.Reverse();

            return chain;
        }
    }
}
