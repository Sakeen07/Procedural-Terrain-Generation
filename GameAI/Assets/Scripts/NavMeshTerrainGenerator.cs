using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]

public class NavMeshTerrainGenerator : MonoBehaviour 
{
    [SerializeField] private float waterHeight = 0.45f;
    [SerializeField] private float mountainHeight = 0.7f;

    

    void UpdateNavMeshAreas()
    {
        NavMeshBuildSettings settings = new NavMeshBuildSettings();
        settings.agentTypeID = NavMesh.GetSettingsByIndex(0).agentTypeID;
        settings.agentRadius = 0.5f;
        settings.agentHeight = 2f;
        settings.minRegionArea = 1;

        NavMeshBuildSource[] sources = new NavMeshBuildSource[1];
        sources[0] = new NavMeshBuildSource();
        sources[0].shape = NavMeshBuildSourceShape.Mesh;
        sources[0].sourceObject = GetComponent<MeshFilter>().sharedMesh;
        sources[0].transform = transform.localToWorldMatrix;
        sources[0].area = 0;

        Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
        Vector3[] vertices = mesh.vertices;
        int[] areas = new int[vertices.Length];
        
        for (int i = 0; i < vertices.Length; i++)
        {
            float height = vertices[i].y;
            if (height < waterHeight)
                areas[i] = NavMesh.GetAreaFromName("Water");
            else if (height > mountainHeight)
                areas[i] = NavMesh.GetAreaFromName("Mountain");
            else
                areas[i] = NavMesh.GetAreaFromName("Walkable");
        }

        sources[0].area = areas[0];
    }
}