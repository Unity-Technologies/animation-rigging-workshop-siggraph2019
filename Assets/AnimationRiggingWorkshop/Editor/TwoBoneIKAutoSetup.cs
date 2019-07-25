using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class TwoBoneIKAutoSetup : Editor
{
    [MenuItem("Animation Rigging/Utilities/Auto-Setup TwoBoneIK from Tip Transform", false, 0)]
    public static void TwoBoneIKAutoSetupSelected(MenuCommand command)
    {
        var selection = Selection.activeObject as GameObject;
        if (!selection)
        {
            Debug.LogWarning("Please select a GameObject with TwoBoneIKConstraint before running auto setup.");
            return;
        }

        var constraint = selection.GetComponent<TwoBoneIKConstraint>();
        if (!constraint)
        {
            Debug.LogWarning("Please select a GameObject with TwoBoneIKConstraint before running auto setup.");
            return;
        }

        if (!constraint.data.tip)
        {
            Debug.LogWarning("Please provide a tip transform for the TwoBoneIKConstraint before running auto setup.");
            return;
        }

        TwoBoneIKAutoSetupUtility(constraint);
    }

    public static void TwoBoneIKAutoSetupUtility(TwoBoneIKConstraint constraint)
    {
        var tip = constraint.data.tip;
 
        if (!constraint.data.mid)
        {
            Undo.RecordObject(constraint, "Setup mid bone for TwoBoneIK");
            constraint.data.mid = tip.parent;
            if (PrefabUtility.IsPartOfPrefabInstance(constraint)) EditorUtility.SetDirty(constraint);
        }

        if (!constraint.data.root)
        {
            Undo.RecordObject(constraint, "Setup root bone for TwoBoneIK");
            constraint.data.root = tip.parent.parent;
            if (PrefabUtility.IsPartOfPrefabInstance(constraint)) EditorUtility.SetDirty(constraint);
        }

        if (!constraint.data.target)
        {
            var target = constraint.transform.Find(constraint.gameObject.name + "_target");
            if (target == null)
            {
                var t = new GameObject();
                Undo.RegisterCreatedObjectUndo(t, "Created target");
                t.name = constraint.gameObject.name + "_target";
                Undo.SetTransformParent(t.transform, constraint.transform, "Set new parent");
                target = t.transform;
                if (PrefabUtility.IsPartOfPrefabInstance(constraint)) EditorUtility.SetDirty(constraint);
            }
            constraint.data.target = target;
        }

        if (!constraint.data.hint)
        {
            var hint = constraint.transform.Find(constraint.gameObject.name + "_hint");
            if (hint == null)
            {
                var t = new GameObject();
                Undo.RegisterCreatedObjectUndo(t, "Created hint");
                t.name = constraint.gameObject.name + "_hint";
                Undo.SetTransformParent(t.transform, constraint.transform, "Set new parent");
                hint = t.transform;
            }
            constraint.data.hint = hint;
            if (PrefabUtility.IsPartOfPrefabInstance(constraint)) EditorUtility.SetDirty(constraint);
        }

        // align target and hint to bones
        constraint.data.target.position = constraint.data.tip.position;
        constraint.data.target.rotation = constraint.data.tip.rotation;

        constraint.data.hint.position = constraint.data.mid.position;
        constraint.data.hint.rotation = constraint.data.mid.rotation;
    }
}
