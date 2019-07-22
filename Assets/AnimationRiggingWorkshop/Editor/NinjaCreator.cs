using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class NinjaCreator : EditorWindow
{
    [MenuItem("GameObject/3D Object/Ninja", false, 10)]
    static void CreateNinja(MenuCommand menuCommand)
    {

        GameObject ninjaPrefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/TestNinja/Content/Characters/Ninja/Prefabs/NinjaRig.prefab",typeof(GameObject));
        GameObject clone  = PrefabUtility.InstantiatePrefab(ninjaPrefab) as GameObject;
       
    }
}
