using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Diagnostics;

public class NewNPCSpawner : MonoBehaviour
{
    [Header("NPC Settings")]
    public GameObject newNPCPrefab;
    public int numberOfNPCs = 3;
    public float spawnRadius = 10f;
    public KeyCode npcPathKey = KeyCode.N;
    
    [Header("Visual Materials")]
    public Material idleMaterial;
    public Material chaseMaterial;
    public Material gatherMaterial;
    public Material fightMaterial;

    private List<GameObject> spawnedNPCs = new List<GameObject>();
    private List<LineRenderer> activePathLines = new List<LineRenderer>();
    private float pathDisplayDuration = 5f;
    private float pathWidth = 0.2f;
    private NavMeshAgent pathfindingAgent;

    void Start()
    {
        pathfindingAgent = gameObject.AddComponent<NavMeshAgent>();
        pathfindingAgent.enabled = false;
        SpawnNPCs();
    }

        void SpawnNPCs()
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        for(int i = 0; i < numberOfNPCs; i++)
        {
            Vector3 position = GetRandomWalkablePosition();
            NavMeshHit hit;
            if(position != Vector3.zero)
            {
                GameObject npc = Instantiate(newNPCPrefab, position, Quaternion.identity);
                npc.name = $"NewNPC_{i+1}";
                
                NewNPC npcComponent = npc.GetComponent<NewNPC>();
                if(npcComponent != null)
                {
                    npcComponent.idleMaterial = idleMaterial;
                    npcComponent.chaseMaterial = chaseMaterial;
                    npcComponent.gatherMaterial = gatherMaterial;
                    npcComponent.fightMaterial = fightMaterial;
                    npcComponent.npcID = $"BOT2_{i+1}";
                }
                
                spawnedNPCs.Add(npc);
            }
        }
        stopwatch.Stop();
        UnityEngine.Debug.Log($"NPC spawning took {stopwatch.ElapsedMilliseconds} ms");
    }

    void Update()
    {
        spawnedNPCs.RemoveAll(npc => npc == null);

        if(Input.GetKeyDown(npcPathKey))
        {
            UnityEngine.Debug.Log("N key pressed - showing BOT2 paths");
            ShowNPCPaths();
        }
    }

    Vector3 GetRandomWalkablePosition()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (!player) return Vector3.zero;

        float minDistance = 300f;
        float maxDistance = 900f;

        for(int i = 0; i < 30; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle.normalized;
            float randomDistance = Random.Range(minDistance, maxDistance);
            
            Vector3 randomPoint = player.transform.position + new Vector3(randomCircle.x, 0, randomCircle.y) * randomDistance;
            
            NavMeshHit hit;
            if(NavMesh.SamplePosition(randomPoint, out hit, 5f, NavMesh.AllAreas))
            {
                float distanceToPlayer = Vector3.Distance(hit.position, player.transform.position);
                if (distanceToPlayer >= minDistance)
                {
                    UnityEngine.Debug.Log($"Found valid item position at distance: {distanceToPlayer}");
                    return hit.position;
                }
            }
        }
        UnityEngine.Debug.LogWarning("Failed to find spawn position");
        return Vector3.zero;
    }

    void ShowNPCPaths()
    {
        ClearPaths();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if(!player) return;

        foreach(GameObject npc in spawnedNPCs)
        {
            if(npc != null)
            {
                NavMeshPath path = new NavMeshPath();
                if(NavMesh.CalculatePath(player.transform.position, npc.transform.position, NavMesh.AllAreas, path))
                {
                    CreatePathLine(path, Color.blue);
                    UnityEngine.Debug.Log($"Drawing path to BOT2 at {npc.transform.position}");
                }
            }
        }

        Invoke("ClearPaths", pathDisplayDuration);
    }

    void CreatePathLine(NavMeshPath path, Color pathColor)
    {
        GameObject pathLine = new GameObject("PathLine");
        LineRenderer line = pathLine.AddComponent<LineRenderer>();
        
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = pathColor;
        line.endColor = pathColor;
        line.startWidth = pathWidth;
        line.endWidth = pathWidth;
        line.positionCount = path.corners.Length;
        
        Vector3[] positions = new Vector3[path.corners.Length];
        for(int i = 0; i < path.corners.Length; i++)
        {
            positions[i] = path.corners[i] + Vector3.up * 0.1f;
        }
        line.SetPositions(positions);
        
        activePathLines.Add(line);
    }

    void ClearPaths()
    {
        foreach (var line in activePathLines)
        {
            Destroy(line.gameObject);
        }
        activePathLines.Clear();
    }

    void OnDestroy()
    {
        ClearPaths();
    }
}