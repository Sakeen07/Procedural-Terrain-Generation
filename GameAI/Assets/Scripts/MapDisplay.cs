using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDisplay : MonoBehaviour
{
    
	public Renderer textureRender;
	public MeshFilter meshFilter;
	public MeshRenderer meshRenderer;
	public MeshCollider meshCollider;

	private NavMeshTerrainGenerator navMeshTerrainGenerator;

	void Awake() {
		if (meshCollider == null) {
            meshCollider = GetComponent<MeshCollider>();
        }
	}

	public void DrawTexture(Texture2D texture) {
		textureRender.sharedMaterial.mainTexture = texture;
		textureRender.transform.localScale = new Vector3 (texture.width, 1, texture.height);
	}

    
	 public void DrawMesh(Mesh mesh, Texture2D texture, bool generateCollider, float colliderOffset) {
        meshFilter.sharedMesh = mesh;
        meshRenderer.sharedMaterial.mainTexture = texture;
        
        if (generateCollider && meshCollider != null) {
            
            Mesh collisionMesh = new Mesh();
            Vector3[] vertices = mesh.vertices;
            for (int i = 0; i < vertices.Length; i++) {
                vertices[i].y += colliderOffset;
            }
            collisionMesh.vertices = vertices;
            collisionMesh.triangles = mesh.triangles;
            collisionMesh.RecalculateNormals();
            
            meshCollider.sharedMesh = collisionMesh;
        }
    }
}
