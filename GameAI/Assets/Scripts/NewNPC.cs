using UnityEngine;
using UnityEngine.AI;
using TMPro;
using System.Linq;

public class NewNPC : MonoBehaviour
{
    public enum NewNPCStates
    {
        Idle,
        Chase,
        Gather,
        Fight
    }

    [Header("UI Settings")]
    [SerializeField] public string npcID = "BOT2";
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;
    private NPCUI npcUI;

    [Header("Combat Settings")]
    [SerializeField] private Transform Player;
    [SerializeField] private float chaseRange = 10f;
    [SerializeField] private float bulletSpeed = 10f;
    [SerializeField] private float fireRate = 2f;
    [SerializeField] private float gatherRange = 15f;

    [Header("Visual Feedback")]
    public Material idleMaterial;
    public Material chaseMaterial;
    public Material gatherMaterial;
    public Material fightMaterial;

    private NewNPCStates currentState = NewNPCStates.Idle;
    private NavMeshAgent navMeshAgent;
    private MeshRenderer meshRenderer;
    private float nextShootTime = 0;
    private Transform allyNPC;

    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        meshRenderer = GetComponent<MeshRenderer>();
        currentHealth = maxHealth;
        
        CreateWorldUI();
        
        if (Player == null)
        {
            Player = GameObject.FindGameObjectWithTag("Player")?.transform;
        }
    }

    void CreateWorldUI()
    {
        GameObject canvasObj = new GameObject($"{npcID}_UI");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;

        canvasObj.transform.SetParent(transform);
        canvasObj.transform.localPosition = Vector3.up * 2;
        canvasObj.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);

        GameObject uiObj = new GameObject("UI_Elements");
        uiObj.transform.SetParent(canvasObj.transform, false);
        
        npcUI = uiObj.AddComponent<NPCUI>();
        npcUI.target = transform;

        npcUI.idText = CreateTextElement(uiObj, "ID_Text", $"ID: {npcID}", new Vector3(0, 15f, 0));
        npcUI.healthText = CreateTextElement(uiObj, "Health_Text", $"Health: {currentHealth}", new Vector3(0, -40f, 0));
        npcUI.statusText = CreateTextElement(uiObj, "Status_Text", $"Status: {currentState}", new Vector3(0, -94f, 0));

        npcUI.UpdateUI(npcID, currentHealth, currentState.ToString());
    }

    private TextMeshProUGUI CreateTextElement(GameObject parent, string name, string initialText, Vector3 localPos)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent.transform, false);
        textObj.transform.localPosition = localPos;
        
        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = initialText;
        tmp.fontSize = 60;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.enableWordWrapping = false;
        tmp.color = Color.white;
        tmp.margin = new Vector4(0, 0, 0, 0);
        tmp.rectTransform.sizeDelta = new Vector2(200, 50);
        
        return tmp;
    }

    void Update()
    {
        if (Player == null) return;
        
        SwitchState();
        
        switch (currentState)
        {
            case NewNPCStates.Idle:
                Idle();
                break;
            case NewNPCStates.Chase:
                Chase();
                break;
            case NewNPCStates.Gather:
                Gather();
                break;
            case NewNPCStates.Fight:
                Fight();
                break;
        }
    }
        public void RespondToAllyCall(Transform caller)
    {
        if (currentState != NewNPCStates.Fight)
        {
            allyNPC = caller;
            currentState = NewNPCStates.Gather;
            Debug.Log($"{npcID} responding to ally call");
        }
    }

    private void SwitchState()
    {
        if (currentHealth <= 0)
        {
            Destroy();
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, Player.position);
        NewNPCStates previousState = currentState;

        if (currentHealth <= 20 && currentState != NewNPCStates.Gather)
        {
            currentState = NewNPCStates.Gather;
            FindNearestAlly();
        }
        else if (currentState == NewNPCStates.Chase && distanceToPlayer <= 5f)
        {
            currentState = NewNPCStates.Fight;
        }
        else if (currentState == NewNPCStates.Fight && distanceToPlayer > 5f)
        {
            currentState = NewNPCStates.Chase;
        }

        if (previousState != currentState)
        {
            UpdateUIAndMaterial();
            Debug.Log($"{npcID} state changed from {previousState} to {currentState}");
        }
    }

    private void UpdateUIAndMaterial()
    {
        npcUI?.UpdateStatus(currentState.ToString());
        
        switch (currentState)
        {
            case NewNPCStates.Idle:
                meshRenderer.material = idleMaterial;
                break;
            case NewNPCStates.Chase:
                meshRenderer.material = chaseMaterial;
                break;
            case NewNPCStates.Gather:
                meshRenderer.material = gatherMaterial;
                break;
            case NewNPCStates.Fight:
                meshRenderer.material = fightMaterial;
                break;
        }
    }

    private bool IsInCombatState()
    {
        return currentState == NewNPCStates.Chase || currentState == NewNPCStates.Fight;
    }


        private void Idle()
    {
        meshRenderer.material = idleMaterial;
        navMeshAgent.isStopped = true;
        
        float distanceToPlayer = Vector3.Distance(transform.position, Player.position);
        if (distanceToPlayer <= chaseRange)
        {
            currentState = NewNPCStates.Chase;
            UpdateUIAndMaterial();
        }
    }

    private void Chase()
    {
        if (GameUI.Instance.IsPlayerInvisible())
        {
            currentState = NewNPCStates.Idle;
            UpdateUIAndMaterial();
            return;
        }

        meshRenderer.material = chaseMaterial;
        navMeshAgent.isStopped = false;
        navMeshAgent.SetDestination(Player.position);

        float distanceToPlayer = Vector3.Distance(transform.position, Player.position);
        if (distanceToPlayer <= 5f)
        {
            currentState = NewNPCStates.Fight;
            UpdateUIAndMaterial();
            Debug.Log($"{npcID} in attack range, switching to fight");
        }
    }

    private void Gather()
    {
        if (GameUI.Instance.IsPlayerInvisible())
        {
            currentState = NewNPCStates.Idle;
            UpdateUIAndMaterial();
            return;
        }
        
        if (allyNPC == null)
        {
            FindNearestAlly();
            return;
        }

        meshRenderer.material = gatherMaterial;
        navMeshAgent.isStopped = false;
        navMeshAgent.SetDestination(allyNPC.position);

        float distanceToAlly = Vector3.Distance(transform.position, allyNPC.position);
        if (distanceToAlly <= 2f)
        {
            currentState = NewNPCStates.Chase;
            var ally = allyNPC.GetComponent<NewNPC>();
            if (ally != null)
            {
                ally.ForceChaseState();
            }
            UpdateUIAndMaterial();
            Debug.Log($"{npcID} met with ally, both switching to chase");
        }
    }

    public void ForceChaseState()
    {
        currentState = NewNPCStates.Chase;
        UpdateUIAndMaterial();
        Debug.Log($"{npcID} forced to chase state by ally");
    }

    private void Fight()
    {
        if (GameUI.Instance.IsPlayerInvisible())
        {
            currentState = NewNPCStates.Idle;
            UpdateUIAndMaterial();
            return;
        }

        meshRenderer.material = fightMaterial;
        navMeshAgent.isStopped = true;
        transform.LookAt(Player);

        float distanceToPlayer = Vector3.Distance(transform.position, Player.position);
        if (distanceToPlayer > 5f)
        {
            currentState = NewNPCStates.Chase;
            UpdateUIAndMaterial();
            Debug.Log($"{npcID} target out of range, switching to chase");
        }
        else if (Time.time > nextShootTime)
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        nextShootTime = Time.time + fireRate;
        
        GameObject bullet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        bullet.name = "NPCBullet";
        bullet.tag = "NPCBullet";
        bullet.transform.position = transform.position + transform.forward;
        bullet.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        
        Rigidbody rb = bullet.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.velocity = transform.forward * bulletSpeed;
        
        SphereCollider bulletCollider = bullet.GetComponent<SphereCollider>();
        bulletCollider.isTrigger = true;
        bulletCollider.radius = 0.2f;
        
        Bullet bulletComponent = bullet.AddComponent<Bullet>();
        bulletComponent.bulletType = Bullet.BulletType.NPC;
        
        Destroy(bullet, 2f);
        Debug.Log($"{npcID} fired bullet");
    }

    private void FindNearestAlly()
    {
        var allies = FindObjectsOfType<NewNPC>()
            .Where(x => x != this && 
                    x.currentHealth > 20 &&
                    x != null)
            .OrderBy(x => Vector3.Distance(transform.position, x.transform.position))
            .FirstOrDefault();

        if (allies != null)
        {
            allyNPC = allies.transform;
            Debug.Log($"{npcID} found ally: {allies.npcID} at distance: {Vector3.Distance(transform.position, allyNPC.position)}");
        }
        else
        {
            Debug.Log($"{npcID} couldn't find any allies");
        }
    }

    public void JoinAlly()
    {
        if (currentState != NewNPCStates.Fight && currentState != NewNPCStates.Chase)
        {
            currentState = NewNPCStates.Chase;
            UpdateUIAndMaterial();
            Debug.Log($"{npcID} joining ally in chase");
        }
    }

    public void HandleAllyDeath()
    {
        allyNPC = null;
        if (currentHealth <= 20)
        {
            currentState = NewNPCStates.Gather;
            FindNearestAlly();
        }
    }

        public void TakeDamage(int damage)
    {
        currentHealth = Mathf.Max(0, currentHealth - damage);
        Debug.Log($"{npcID} took damage. Current health: {currentHealth}");
        
        if(npcUI != null)
        {
            npcUI.UpdateHealth(currentHealth);
        }

        if(currentHealth <= 20 && currentState != NewNPCStates.Gather)
        {
            currentState = NewNPCStates.Gather;
            FindNearestAlly();
            UpdateUIAndMaterial();
            Debug.Log($"{npcID} health low, switching to gather state");
        }
        else if(currentHealth <= 0)
        {
            Destroy();
        }
    }

    private void Destroy()
    {
        Debug.Log($"{npcID} died!");
        if (allyNPC != null)
        {
            NewNPC ally = allyNPC.GetComponent<NewNPC>();
            ally?.HandleAllyDeath();
        }
        Destroy(gameObject);
    }
    public void StartFighting()
    {
        currentState = NewNPCStates.Fight;
        Debug.Log($"{npcID} starting to fight alongside ally");
    }

    void OnEnable()
        {
            GameUI.OnCoinPickup += OnCoinCollected;
            
        }

    void OnDisable()
    {
        GameUI.OnCoinPickup -= OnCoinCollected;
    }

    private void OnCoinCollected()
    {
        currentState = NewNPCStates.Chase;
        UpdateUIAndMaterial();
        Debug.Log($"{npcID} alerted by coin pickup");
    }
}