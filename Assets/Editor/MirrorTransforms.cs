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
}
