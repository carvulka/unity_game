using UnityEngine;
using UnityEditor;
using System.Text;

public class CARVULKA : EditorWindow
{
    [MenuItem("carvulka/epr")]
    public static void epr()
    {
        GameObject[] selected_objects = Selection.gameObjects;
        if (selected_objects.Length == 0) { return; }
        
        StringBuilder string_builder = new StringBuilder();
        foreach (GameObject selected_object in selected_objects)
        {
            Vector3 pos = selected_object.transform.position;
            Vector3 rot = selected_object.transform.rotation.eulerAngles;
            string_builder.AppendLine("\t\t<prop_spawn_point prop_pool_id=\"\">");
            string_builder.AppendLine($"\t\t\t<position><x>{pos.x}</x><y>{pos.y}</y><z>{pos.z}</z></position>");
            string_builder.AppendLine($"\t\t\t<rotation><x>{rot.x}</x><y>{rot.y}</y><z>{rot.z}</z></rotation>");
            string_builder.AppendLine("\t\t</prop_spawn_point>");
        }
        GUIUtility.systemCopyBuffer = string_builder.ToString();
    }
}
