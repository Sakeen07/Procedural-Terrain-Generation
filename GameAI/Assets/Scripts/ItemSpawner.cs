using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Diagnostics;

public class ItemSpawner : MonoBehaviour 
{

    public GameObject magicPotionPrefab;
    public GameObject healthPrefab;
    public GameObject armorPrefab;
    public GameObject weaponPrefab;
    public GameObject coinPrefab;
    public GameObject boostPrefab;
    public int numberOfHealthItems = 3;
    public int numberOfPotions = 3;
    public int numberOfArmorItems = 3;
     public int numberOfWeapons = 3;
     public int numberOfCoins = 3;
     public int numberOfBoosts = 3;
     public List<GameObject> spawnedBoosts = new List<GameObject>();
     public List<GameObject> spawnedCoins = new List<GameObject>();
     public List<GameObject> spawnedPotions = new List<GameObject>();
     
    private NavMeshAgent pathfindingAgent;
    
    public List<GameObject> spawnedHealthItems = new List<GameObject>();
    public List<GameObject> spawnedArmorItems = new List<GameObject>();
    private List<LineRenderer> activePathLines = new List<LineRenderer>();
    public List<GameObject> spawnedWeapons = new List<GameObject>();

    private float pathDisplayDuration = 5f;
    private float pathWidth = 0.2f;

    [Header("Pickup Settings")]
    public float pickupRadius = 2f;
    public KeyCode healthPathKey = KeyCode.H;
    public KeyCode armorPathKey = KeyCode.J;
    public KeyCode weaponPathKey = KeyCode.K;
    public KeyCode coinPathKey = KeyCode.L;
    public KeyCode boostPathKey = KeyCode.B;
    public KeyCode potionPathKey = KeyCode.P;

    void Start()
    {
        pathfindingAgent = gameObject.AddComponent<NavMeshAgent>();
        pathfindingAgent.enabled = false;
        SpawnInitialItems();
        UnityEngine.Debug.Log("ItemSpawner initialized");
    }

    void SpawnInitialItems()
    {
        foreach(var item in spawnedHealthItems) Destroy(item);
        foreach(var item in spawnedArmorItems) Destroy(item);
        foreach(var item in spawnedWeapons) Destroy(item);
        foreach(var item in spawnedCoins) Destroy(item);
        foreach(var item in spawnedBoosts) Destroy(item);
        foreach(var item in spawnedPotions) Destroy(item);

        spawnedPotions.Clear();
        spawnedHealthItems.Clear();
        spawnedArmorItems.Clear();
        spawnedWeapons.Clear();
        spawnedCoins.Clear();
        spawnedBoosts.Clear();


        for(int i = 0; i < numberOfHealthItems; i++)
        {
            SpawnItem(healthPrefab, spawnedHealthItems);
        }
        for(int i = 0; i < numberOfArmorItems; i++)
        {
            SpawnItem(armorPrefab, spawnedArmorItems);
        }
        for(int i = 0; i < numberOfWeapons; i++)
        {
            SpawnItem(weaponPrefab, spawnedWeapons);
        }
         for(int i = 0; i < numberOfCoins; i++)
        {
            SpawnItem(coinPrefab, spawnedCoins);
        }
        for(int i = 0; i < numberOfBoosts; i++)
        {
            SpawnItem(boostPrefab, spawnedBoosts);
        }
        for(int i = 0; i < numberOfPotions; i++)
        {
            SpawnItem(magicPotionPrefab, spawnedPotions);
        }
    }

    void SpawnItem(GameObject prefab, List<GameObject> itemList)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        Vector3 position = GetRandomWalkablePosition();
        if(position != Vector3.zero)
        {
            GameObject item = Instantiate(prefab, position + Vector3.up * 1f, Quaternion.identity);
            item.transform.parent = transform;
            itemList.Add(item);
            UnityEngine.Debug.Log($"Spawned {prefab.name} at {position}");
        }
        stopwatch.Stop();
        UnityEngine.Debug.Log($"NPC spawning took {stopwatch.ElapsedMilliseconds} ms");
    }

    void Update()
    {
        if(Input.GetKeyDown(healthPathKey))
        {
            UnityEngine.Debug.Log("H key pressed - showing health paths");
            ShowPaths(spawnedHealthItems, Color.red);
        }
        else if(Input.GetKeyDown(armorPathKey))
        {
            UnityEngine.Debug.Log("J key pressed - showing armor paths");
            ShowPaths(spawnedArmorItems, Color.gray);
        }
        else if(Input.GetKeyDown(weaponPathKey))
        {
            UnityEngine.Debug.Log("K key pressed - showing weapon paths");
            ShowPaths(spawnedWeapons, Color.yellow);
        }
        else if(Input.GetKeyDown(coinPathKey))
        {
            UnityEngine.Debug.Log("L key pressed - showing coin paths");
            ShowPaths(spawnedCoins, Color.yellow);
        }
        else if(Input.GetKeyDown(boostPathKey))
        {
            UnityEngine.Debug.Log("B key pressed - showing boost paths");
            ShowPaths(spawnedBoosts, Color.blue);
        }
        else if(Input.GetKeyDown(potionPathKey))
        {
        UnityEngine.Debug.Log("P key pressed - showing potion paths");
            ShowPaths(spawnedPotions, Color.magenta);
        }

        CheckForPickups();
    }

    void CheckForPickups()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if(!player) return;

        CheckItemsInRange(player, spawnedHealthItems, healthPrefab, "Health");
        CheckItemsInRange(player, spawnedArmorItems, armorPrefab, "Armor");
        CheckItemsInRange(player, spawnedWeapons, weaponPrefab, "Weapon");
        CheckItemsInRange(player, spawnedCoins, coinPrefab, "Coin");
        CheckItemsInRange(player, spawnedBoosts, boostPrefab, "Boost");
        CheckItemsInRange(player, spawnedPotions, magicPotionPrefab, "MagicPotion");

    }

    void ShowPaths(List<GameObject> items, Color pathColor)
    {
        foreach (var line in activePathLines)
        {
            Destroy(line.gameObject);
        }
        activePathLines.Clear();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if(!player) return;

        foreach(GameObject item in items)
        {
            if(item != null)
            {
                NavMeshPath path = new NavMeshPath();
                if(NavMesh.CalculatePath(player.transform.position, item.transform.position, NavMesh.AllAreas, path))
                {
                    CreatePathLine(path, pathColor);
                    UnityEngine.Debug.Log($"Drawing path to item at {item.transform.position}");
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

    public void RespawnItem(GameObject prefab, List<GameObject> itemList)
    {
        SpawnItem(prefab, itemList);
    }

    void CheckItemsInRange(GameObject player, List<GameObject> items, GameObject prefab, string itemType)
    {
        List<GameObject> itemsToRemove = new List<GameObject>();

        foreach(GameObject item in items)
        {
            if(item != null)
            {
                float distance = Vector3.Distance(player.transform.position, item.transform.position);
                if(distance <= pickupRadius)
                {
                    if(itemType == "Health")
                    {
                        GameUI.Instance.AddHealth();
                    }
                    else if(itemType == "Armor")
                    {
                        GameUI.Instance.PickUpArmor();
                    }
                    else if(itemType == "Weapon")
                    {
                        GameUI.Instance.PickUpWeapon();
                    }
                    else if(itemType == "Coin")
            {
                GameUI.Instance.AddPoint();
            }
            else if(itemType == "Boost")
        {
            GameUI.Instance.ActivateBoost();
        }
        else if(itemType == "MagicPotion")
        {
            GameUI.Instance.ActivateMagicPotion();
        }
                    
                    UnityEngine.Debug.Log($"Picking up {itemType}!");
                    itemsToRemove.Add(item);
                    Destroy(item);
                }
            }
        }

        foreach(GameObject item in itemsToRemove)
        {
            items.Remove(item);
            SpawnItem(prefab, items);
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
            // Get random angle
            float randomAngle = Random.Range(0f, 360f);
            float randomDistance = Random.Range(minDistance, maxDistance);
            
            // Calculate position using angle
            Vector3 randomDirection = Quaternion.Euler(0, randomAngle, 0) * Vector3.forward;
            Vector3 randomPoint = player.transform.position + randomDirection * randomDistance;
            
            NavMeshHit hit;
            if(NavMesh.SamplePosition(randomPoint, out hit, 10f, NavMesh.AllAreas))
            {
                float actualDistance = Vector3.Distance(hit.position, player.transform.position);
                if (actualDistance >= minDistance)
                {
                    UnityEngine.Debug.Log($"Spawning item at distance: {actualDistance}");
                    return hit.position;
                }
            }
        }
        UnityEngine.Debug.LogWarning("Failed to find spawn position");
        return Vector3.zero;
    }

}