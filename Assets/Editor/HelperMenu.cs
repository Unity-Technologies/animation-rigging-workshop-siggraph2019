using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class MirrorTransformWindow : EditorWindow
{
    private bool m_FlipX = true;
    private bool m_FlipY = false;
    private bool m_FlipZ = false;

    private float m_Distance = 0f;

    private Vector4 m_Plane = new Vector4(1f, 0f, 0f, 0f);

    [MenuItem("Animation Rigging/Utilities/Mirror Transforms", false, 0)]
    static void ShowWindow()
    {
        MirrorTransformWindow window = EditorWindow.GetWindow<MirrorTransformWindow>();
        window.position = new Rect(Screen.width / 2, Screen.height / 2, 250, 150);
        window.ShowUtility();
    }

    public void OnGUI()
    {
        EditorGUIUtility.labelWidth = 75;

        EditorGUILayout.LabelField("Plane");
        if (EditorGUILayout.Toggle("X", m_FlipX, EditorStyles.radioButton))
        {
            m_FlipX = true;
            m_FlipY = m_FlipZ = false;

        }
        if (EditorGUILayout.Toggle("Y", m_FlipY, EditorStyles.radioButton))
        {
            m_FlipY = true;
            m_FlipX = m_FlipZ = false;
        }
        if (EditorGUILayout.Toggle("Z", m_FlipZ, EditorStyles.radioButton))
        {
            m_FlipZ = true;
            m_FlipX = m_FlipY = false;
        }

        m_Distance = EditorGUILayout.FloatField("Distance", m_Distance);

        GUILayout.Space(30);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Apply"))
            Apply();
        if (GUILayout.Button("Close"))
            Close();
        GUILayout.EndHorizontal();
    }

    public void Apply()
    {
        Transform[] transforms = Selection.transforms;

        List<Transform> allTransforms = new List<Transform>(transforms.Length);
        foreach(var transform in transforms)
        {
            allTransforms.Add(transform);
            allTransforms.AddRange(transform.GetComponentsInChildren<Transform>(true));
        }

        Vector4 plane = new Vector4(m_FlipX ? 1f : 0f, m_FlipY ? 1f : 0f, m_FlipZ ? 1f : 0f, -m_Distance);
        float xMult = m_FlipX ? 1f : -1f;
        float yMult = m_FlipY ? 1f : -1f;
        float zMult = m_FlipZ ? 1f : -1f;

        Matrix4x4 matrix = CalculateReflectionMatrix(plane);

        List<Vector3> positions = new List<Vector3>(allTransforms.Select((transform) => transform.position));
        List<Vector3> eulerAngles = new List<Vector3>(allTransforms.Select((transform) => transform.eulerAngles));

        for (int i = 0; i < allTransforms.Count; ++i)
        {
            Transform transform = allTransforms[i];
            Vector3 position = positions[i];
            Vector3 angles = eulerAngles[i];

            Undo.RecordObject(transform, "Mirror transforms");
            transform.position = (matrix.MultiplyPoint(positions[i]));
            transform.eulerAngles = new Vector3(xMult * angles.x, yMult * angles.y, zMult * angles.z);
        }
    }

    // Calculates reflection matrix around the given plane
	private static Matrix4x4 CalculateReflectionMatrix (Vector4 plane)
	{
        var reflectionMat = Matrix4x4.zero;

		reflectionMat.m00 = (1F - 2F*plane[0]*plane[0]);
		reflectionMat.m01 = (   - 2F*plane[0]*plane[1]);
		reflectionMat.m02 = (   - 2F*plane[0]*plane[2]);
		reflectionMat.m03 = (   - 2F*plane[3]*plane[0]);

		reflectionMat.m10 = (   - 2F*plane[1]*plane[0]);
		reflectionMat.m11 = (1F - 2F*plane[1]*plane[1]);
		reflectionMat.m12 = (   - 2F*plane[1]*plane[2]);
		reflectionMat.m13 = (   - 2F*plane[3]*plane[1]);

		reflectionMat.m20 = (   - 2F*plane[2]*plane[0]);
		reflectionMat.m21 = (   - 2F*plane[2]*plane[1]);
		reflectionMat.m22 = (1F - 2F*plane[2]*plane[2]);
		reflectionMat.m23 = (   - 2F*plane[3]*plane[2]);

		reflectionMat.m30 = 0F;
		reflectionMat.m31 = 0F;
		reflectionMat.m32 = 0F;
		reflectionMat.m33 = 1F;

        return reflectionMat;
	}

    [MenuItem("Animation Rigging/Utilities/Auto-Setup TwoBoneIK from Tip Transform", false, 0)]
    public static void TwoBoneIKAutoSetup2(MenuCommand command)
    {
        var selection = Selection.activeObject as GameObject;
        if (!selection)
        {
            Debug.LogWarning("Please select a TwoBoneIK before running auto setup.");
            return;
        }

        var constraint = selection.GetComponent<TwoBoneIKConstraint>() ;// command.context as UnityEngine.Animations.Rigging.TwoBoneIKConstraint;
        var tip = constraint.data.tip ;// constraint.data.tip;
        var animator = constraint.GetComponentInParent<Animator>()?.transform;

        if (!tip)
        {
            Debug.LogWarning("Please provide a tip before running auto setup.");
            return;
        }

        if (!constraint.data.mid)
        {
            Undo.RecordObject(constraint, "Setup mid bone for TwoBoneIK");
            constraint.data.mid = tip.parent;
        }

        if (!constraint.data.root)
        {
            Undo.RecordObject(constraint, "Setup root bone for TwoBoneIK");
            constraint.data.root = tip.parent.parent;
        }

        if (!constraint.data.target)
        {
            var target = constraint.transform.Find(constraint.gameObject.name + "_target");
            if (target == null)
            {
                var t = new GameObject();
                Undo.RegisterCreatedObjectUndo(t, "Created target");
                t.name = constraint.gameObject.name + "_target";
                t.transform.localScale = .1f * t.transform.localScale;
                Undo.SetTransformParent(t.transform, constraint.transform, "Set new parent");
                target = t.transform;
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
                t.transform.localScale = .1f * t.transform.localScale;
                Undo.SetTransformParent(t.transform, constraint.transform, "Set new parent");
                hint = t.transform;
            }
            constraint.data.hint = hint;
        }

        // align target and hint to bones
        constraint.data.target.position = constraint.data.tip.position;
        constraint.data.target.rotation = constraint.data.tip.rotation;

        constraint.data.hint.position = constraint.data.mid.position;
        constraint.data.hint.rotation = constraint.data.mid.rotation;
    }


}
