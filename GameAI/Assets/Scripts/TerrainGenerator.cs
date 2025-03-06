using UnityEngine;
using System.Diagnostics;

[System.Serializable]
public struct TerrainType {
    public string name;
    public float height;
    public Color color;
}

public class TerrainGenerator : MonoBehaviour {
    public enum DrawMode { NoiseMap, ColorMap, Mesh };
    public DrawMode drawMode;

    public const int mapChunkSize = 31;
    [Range(0, 6)]
    public int levelOfDetail = 0;

    [Header("Noise Settings")]
    public float noiseScale = 25f;
    public int octaves = 6;
    [Range(0, 1)]
    public float persistance = 0.5f;
    public float lacunarity = 2f;
    
    public int seed;
    public Vector2 offset;

    [Header("Terrain Settings")]
    public float meshHeightMultiplier = 30f;
    public AnimationCurve meshHeightCurve;

    [Header("Quality Settings")]
    public int meshResolution = 31;
    public int textureResolution = 512;
    [Range(1, 4)]
    public int smoothingPasses = 2;

    [Header("Collision Settings")]
    public float colliderHeightOffset = 0.1f;
    public bool generateColliderMesh = true;

    public bool autoUpdate;

    [Header("Terrain Types")]
    public TerrainType[] regions = new TerrainType[] {
        new TerrainType() { name = "Water Deep", height = 0.3f, color = new Color(0.08f, 0.18f, 0.31f) },
        new TerrainType() { name = "Water Shallow", height = 0.4f, color = new Color(0.15f, 0.32f, 0.48f) },
        new TerrainType() { name = "Sand", height = 0.45f, color = new Color(0.93f, 0.85f, 0.63f) },
        new TerrainType() { name = "Grass", height = 0.55f, color = new Color(0.31f, 0.49f, 0.16f) },
        new TerrainType() { name = "Grass Hill", height = 0.6f, color = new Color(0.27f, 0.42f, 0.14f) },
        new TerrainType() { name = "Rock", height = 0.7f, color = new Color(0.47f, 0.43f, 0.45f) },
        new TerrainType() { name = "Rock High", height = 0.9f, color = new Color(0.43f, 0.39f, 0.39f) },
        new TerrainType() { name = "Snow", height = 1.0f, color = new Color(0.97f, 0.97f, 0.97f) }
    };

    [System.Obsolete]
    void Start() {
        GenerateTerrain();
    }


    [System.Obsolete]
    public void GenerateTerrain() {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        float[,] noiseMap = Noise.GenerateNoiseMap(
            meshResolution, 
            meshResolution, 
            seed, 
            noiseScale, 
            octaves, 
            persistance, 
            lacunarity, 
            offset
        );

        for (int i = 0; i < smoothingPasses; i++) {
            noiseMap = SmoothNoiseMap(noiseMap);
        }

        Color[] colorMap = new Color[meshResolution * meshResolution];
        for (int y = 0; y < meshResolution; y++) {
            for (int x = 0; x < meshResolution; x++) {
                float currentHeight = noiseMap[x, y];
                for (int i = 0; i < regions.Length; i++) {
                    if (currentHeight <= regions[i].height) {
                        colorMap[y * meshResolution + x] = regions[i].color;
                        break;
                    }
                }
            }
        }

        MapDisplay display = FindObjectOfType<MapDisplay>();
        if (drawMode == DrawMode.NoiseMap) {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
        }
        else if (drawMode == DrawMode.ColorMap) {
            display.DrawTexture(TextureGenerator.TextureFromColorMap(colorMap, meshResolution, meshResolution));
        }
        else if (drawMode == DrawMode.Mesh) {
            MeshData meshData = MeshGenerator.GenerateTerrainMesh(
                noiseMap, 
                meshHeightMultiplier, 
                meshHeightCurve, 
                levelOfDetail
            );

            Mesh mesh = meshData.CreateMesh();
            display.DrawMesh(
                mesh,
                TextureGenerator.TextureFromColorMap(colorMap, meshResolution, meshResolution),
                generateColliderMesh,
                colliderHeightOffset
            );
        }

        stopwatch.Stop();
        UnityEngine.Debug.Log($"Terrain generation took {stopwatch.ElapsedMilliseconds} ms");
    }

    private float[,] SmoothNoiseMap(float[,] noiseMap) {
        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);
        float[,] smoothedMap = new float[width, height];
        
        int smoothingRadius = 2;
        
        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                float sum = 0f;
                int count = 0;
                
                for (int offsetY = -smoothingRadius; offsetY <= smoothingRadius; offsetY++) {
                    for (int offsetX = -smoothingRadius; offsetX <= smoothingRadius; offsetX++) {
                        int sampleX = x + offsetX;
                        int sampleY = y + offsetY;
                        
                        if (sampleX >= 0 && sampleX < width && sampleY >= 0 && sampleY < height) {
                            sum += noiseMap[sampleX, sampleY];
                            count++;
                        }
                    }
                }
                
                smoothedMap[x, y] = sum / count;
            }
        }
        
        return smoothedMap;
    }

    void OnValidate() {
        if (lacunarity < 1) {
            lacunarity = 1;
        }
        if (octaves < 0) {
            octaves = 0;
        }
    }
}