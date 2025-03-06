using UnityEngine;
using UnityEngine.AI;
using TMPro;

public class NPC : MonoBehaviour
{
    public enum NPCStates
{
    Patrol,
    Chase,
    Attack,
    Retreat
}

    [Header("UI Settings")]
    [SerializeField] public string npcID = "BOT1";
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;
    private NPCUI npcUI;

    [Header("Patrol Settings")]
    [SerializeField] private int NumberOfPatrolPoints = 5;
    [SerializeField] private float PatrolRadius = 20f;
    [SerializeField] private Vector3[] PatrolPoints;
    
    [Header("Combat Settings")]
    [SerializeField] private Transform Player;
    [SerializeField] private float ChaseRange = 7f;
    [SerializeField] private float AttackRange = 4f;
    [SerializeField] private float BulletSpeed = 10f;
    [SerializeField] private float FireRate = 2f;

    [Header("Visual Feedback")]
    [SerializeField] public Material PatrolMaterial;
    [SerializeField] public Material ChaseMaterial;
    [SerializeField] public Material AttackMaterial;
    [SerializeField] public Material RetreatMaterial;

    private int nextPatrolPoint = 0;
    private NPCStates currentState = NPCStates.Patrol;
    private NavMeshAgent navMeshAgent;
    private MeshRenderer meshRenderer;
    private float nextShootTime = 0;

    private bool isAlertedByCoin = false;


    private void SwitchState()
    {
    if (GameUI.Instance.IsPlayerInvisible())
    {
        currentState = NPCStates.Patrol;
        return;
    }

    float distanceToPlayer = Vector3.Distance(transform.position, Player.position);

    if (currentHealth <= 20)
    {
        currentState = NPCStates.Retreat;
        return;
    }

    if (isAlertedByCoin)
        {
            if (distanceToPlayer <= AttackRange)
            {
                currentState = NPCStates.Attack;
            }
            else
            {
                currentState = NPCStates.Chase;
            }
        }
        else
        {
            // Normal state transitions
            if (distanceToPlayer <= AttackRange)
            {
                currentState = NPCStates.Attack;
            }
            else if (distanceToPlayer <= ChaseRange)
            {
                currentState = NPCStates.Chase;
            }
            else
            {
                currentState = NPCStates.Patrol;
            }
        }

    npcUI?.UpdateStatus(currentState.ToString());
    }

    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        meshRenderer = GetComponent<MeshRenderer>();
        currentHealth = maxHealth;
        
        GeneratePatrolPoints();
        CreateWorldUI();
        
        if (PatrolPoints.Length > 0)
        {
            navMeshAgent.SetDestination(PatrolPoints[nextPatrolPoint]);
        }

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

     void GeneratePatrolPoints()
    {
        PatrolPoints = new Vector3[NumberOfPatrolPoints];
        
        for (int i = 0; i < NumberOfPatrolPoints; i++)
        {
            Vector3 randomPoint = GetRandomWalkablePoint();
            if (randomPoint != Vector3.zero)
            {
                PatrolPoints[i] = randomPoint;
                Debug.Log($"Patrol point {i} generated at: {randomPoint}");
            }
        }
    }

    Vector3 GetRandomWalkablePoint()
    {
        for (int attempts = 0; attempts < 30; attempts++)
        {
            Vector3 randomPoint = transform.position + UnityEngine.Random.insideUnitSphere * PatrolRadius;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, PatrolRadius, NavMesh.AllAreas))
            {
                return hit.position;
            }
        }
        Debug.LogWarning("Could not find valid patrol point");
        return Vector3.zero;
    }

     void Update()
     {
        if (Player == null) return;
        
        SwitchState();
        
        switch (currentState)
        {
            case NPCStates.Patrol:
                Patrol();
                break;
            case NPCStates.Chase:
                Chase();
                break;
            case NPCStates.Attack:
                Attack();
                break;
            case NPCStates.Retreat:
                Retreat();
                break;
            default:
                Patrol();
                break;
        }
    }

    private void Attack()
    {
        if (GameUI.Instance.IsPlayerInvisible())
        {
            currentState = NPCStates.Patrol;
            return;
        }
        navMeshAgent.SetDestination(transform.position);
        meshRenderer.material = AttackMaterial;
        npcUI?.UpdateStatus("Attack");
        float distanceToPlayer = Vector3.Distance(transform.position, Player.position);
        
        if (distanceToPlayer > AttackRange)
        {
            currentState = NPCStates.Chase;
            return;
        }

        navMeshAgent.SetDestination(transform.position);
        meshRenderer.material = AttackMaterial;

        if (Time.time > nextShootTime)
        {
            nextShootTime = Time.time + FireRate;
            
            GameObject bullet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bullet.name = "NPCBullet";
            bullet.tag = "NPCBullet";
            bullet.transform.position = transform.position + transform.forward * 1f;
            bullet.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
            
            Rigidbody rb = bullet.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.velocity = transform.forward * BulletSpeed;
            
            SphereCollider bulletCollider = bullet.GetComponent<SphereCollider>();
            bulletCollider.isTrigger = true;
            bulletCollider.radius = 0.2f;
            
            bullet.AddComponent<Bullet>();
            
            Destroy(bullet, 2f);
            Debug.Log("NPC fired bullet at player");
        }
    }

    private void Chase(){
        if (GameUI.Instance.IsPlayerInvisible())
        {
            currentState = NPCStates.Patrol;
            return;
        }

            navMeshAgent.SetDestination(Player.position);
        meshRenderer.material = ChaseMaterial;
        npcUI?.UpdateStatus("Chase");

        if (Vector3.Distance(transform.position, Player.position) < ChaseRange) {

            navMeshAgent.SetDestination(Player.position);
            meshRenderer.material = ChaseMaterial;

        }
        
        if (Vector3.Distance(transform.position, Player.position) < AttackRange)
        {
            currentState = NPCStates.Attack;
        }
        else if (Vector3.Distance(transform.position, Player.position) > ChaseRange)
        {
            currentState = NPCStates.Patrol;
            meshRenderer.material = PatrolMaterial;
        }
    }

    private void Patrol()
    {
        if (Vector3.Distance(transform.position, PatrolPoints[nextPatrolPoint]) < 1)
        {
             meshRenderer.material = PatrolMaterial;
             npcUI?.UpdateStatus("Patrol");

            nextPatrolPoint = (nextPatrolPoint + 1) % PatrolPoints.Length;
            navMeshAgent.SetDestination(PatrolPoints[nextPatrolPoint]);
        
        } else{
            if (Vector3.Distance(transform.position, Player.position) < ChaseRange)
            {
                currentState = NPCStates.Chase;
            }
            else{
                navMeshAgent.SetDestination(PatrolPoints[nextPatrolPoint]);
            }
        }

    }

    private void Retreat()
    {
        meshRenderer.material = RetreatMaterial;
        npcUI?.UpdateStatus("Retreat");

        Vector3 retreatDirection = transform.position - Player.position;
        retreatDirection.y = 0;
        retreatDirection.Normalize();

        Vector3 retreatDestination = transform.position + retreatDirection * PatrolRadius;
        
        NavMeshHit hit;
        if (NavMesh.SamplePosition(retreatDestination, out hit, PatrolRadius, NavMesh.AllAreas))
        {
            navMeshAgent.SetDestination(hit.position);
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth = Mathf.Max(0, currentHealth - damage);
        if(npcUI != null)
        {
            npcUI.UpdateHealth(currentHealth);
        }

        if(currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log($"{npcID} has died!");
        Destroy(gameObject);
    }

    void OnCollisionEnter(Collision collision){
        if (collision.gameObject.name == "Player"){
            meshRenderer.material = RetreatMaterial;
        }
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
         isAlertedByCoin = true;
        currentState = NPCStates.Chase;
        meshRenderer.material = ChaseMaterial;
        npcUI?.UpdateStatus("Chase");
        Debug.Log($"{npcID} alerted by coin pickup, ignoring chase range");
    }
}