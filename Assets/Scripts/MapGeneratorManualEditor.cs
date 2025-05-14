using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapGeneratorManual))]
public class MapGeneratorManualEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        MapGeneratorManual generator = (MapGeneratorManual)target;
        if (GUILayout.Button("Generate Manual Map"))
        {
            generator.GenerateManualMap();
        }
    }
}
