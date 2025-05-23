using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

[CustomEditor(typeof(HouseGen))]
public class HouseGenEditor : Editor
{
    public override void OnInspectorGUI()
    {
        HouseGen hGen = (HouseGen)target;

        if (GUILayout.Button("Generate"))
        {
            hGen.GenerateHouse();
        }
    }
}
