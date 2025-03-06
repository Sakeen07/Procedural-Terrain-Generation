using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(TerrainGenerator))]
public class MapGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        TerrainGenerator mapGen = (TerrainGenerator)target;

        if (DrawDefaultInspector()) {
            if (mapGen.autoUpdate)
            {
                mapGen.GenerateTerrain();
            }
        }

        if (GUILayout.Button("Generate"))
        {
            mapGen.GenerateTerrain();
        }   

    }
}