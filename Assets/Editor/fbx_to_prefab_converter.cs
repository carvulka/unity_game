using UnityEngine;
using UnityEditor;
using System.IO;

public class FBX_TO_PREFAB_CONVERTER : EditorWindow
{
    [MenuItem("carvulka/convert_fbx_to_prefab")]
    public static void convert()
    {
        GameObject[] selected_objects = Selection.gameObjects;

        foreach (GameObject fbx in selected_objects)
        {
            string path = AssetDatabase.GetAssetPath(fbx);
            if (!path.ToLower().EndsWith(".fbx")) continue;

            GameObject child_object = (GameObject)PrefabUtility.InstantiatePrefab(fbx);
            GameObject parent_object = new GameObject(fbx.name);
            parent_object.AddComponent<Rigidbody>();

            MeshCollider collider = child_object.AddComponent<MeshCollider>();
            collider.convex = true;
            
            Renderer[] renderers = child_object.GetComponentsInChildren<Renderer>();
            if (renderers.Length > 0)
            {
                Bounds compound_bounds = renderers[0].bounds;
                for (int n = 1; n < renderers.Length; n = n + 1)
                {
                    compound_bounds.Encapsulate(renderers[n].bounds);
                }
                float offset = child_object.transform.position.y - compound_bounds.min.y;
                child_object.transform.SetParent(parent_object.transform);
                child_object.transform.localPosition = new Vector3(0, offset, 0);
            }

            string prefab_path = Path.Combine("Assets/prefabs", parent_object.name + ".prefab");
            PrefabUtility.SaveAsPrefabAssetAndConnect(parent_object, prefab_path, InteractionMode.AutomatedAction);

            DestroyImmediate(parent_object);
        }
    }
}
