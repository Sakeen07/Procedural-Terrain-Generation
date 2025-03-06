using UnityEngine;
using UnityEngine.AI;

public class AgentController : MonoBehaviour 
{

    [Header("Mouse Look Settings")]
    [SerializeField] private float mouseSensitivity = 2f;
    private float mouseX;
    private float mouseY;
    private float xRotation = 0f;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 120f;
    
    [Header("Terrain Settings")]
    [SerializeField] private TerrainGenerator terrainGenerator;
    private const float MIN_WALKABLE_HEIGHT = 0.45f;
    private const float MAX_WALKABLE_HEIGHT = 0.7f;

    [Header("Weapon Settings")]
    [SerializeField] private float bulletSpeed = 20f;
    [SerializeField] private float fireRate = 0.5f;
    private bool hasWeapon = false;
    private float nextFireTime = 0f;
    
    private NavMeshAgent agent;
    private CharacterController characterController;
    private Camera mainCamera;
    private ItemSpawner itemSpawner;

    private Vector3 initialPosition;
    private Quaternion initialRotation;

    void Start()
    {
    agent = GetComponent<NavMeshAgent>();
    characterController = GetComponent<CharacterController>();
    mainCamera = Camera.main;
    itemSpawner = FindObjectOfType<ItemSpawner>();
    
    initialPosition = transform.position;
    initialRotation = transform.rotation;
    
    if (terrainGenerator == null)
        terrainGenerator = FindObjectOfType<TerrainGenerator>();

    ConfigureAgent();
    Debug.Log("Agent initialized");

    Cursor.lockState = CursorLockMode.Locked;
    Cursor.visible = false;
    }

    void ConfigureAgent()
    {
        if (agent == null) return;
        
        agent.updateRotation = false;
        agent.updatePosition = true;
        agent.speed = moveSpeed;
        agent.angularSpeed = rotationSpeed;
        agent.acceleration = 8f;
        Debug.Log("Agent configured");
    }


    void HandleMovement()
    {
        mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        if (mainCamera != null)
        {
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);
            mainCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }

        float horizontal = Input.GetAxisRaw("Horizontal"); 
        float vertical = Input.GetAxisRaw("Vertical");    
        float currentMoveSpeed = moveSpeed;

        Vector3 moveDirection = Vector3.zero;

        if (Input.GetKeyDown(KeyCode.S))
        {
            transform.Rotate(Vector3.up * 180f);
        }
        else
        {
            
            if (vertical > 0)
            {
                moveDirection += transform.forward;
            }
            
            if (horizontal != 0)
            {
                moveDirection += transform.right * horizontal;
            }
        }

        moveDirection.Normalize();

        if (moveDirection.magnitude >= 0.1f)
        {
            if (GameUI.Instance.HasBoost())
            {
                currentMoveSpeed *= 5f;
            }

            if (agent.isOnNavMesh)
            {
                agent.Move(moveDirection * currentMoveSpeed * Time.deltaTime);
            }
        }
    }

    void ToggleCursorLock()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? 
                CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = !Cursor.visible;
        }
    }

        void HandleShooting()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Space pressed for shooting");
            
            if (GameUI.Instance == null)
            {
                Debug.LogError("GameUI Instance is null!");
                return;
            }

            bool canUseWeapon = GameUI.Instance.UseWeapon();
            Debug.Log($"Can use weapon: {canUseWeapon}");

            if (canUseWeapon)
            {
                Shoot();
            }
            else
            {
                Debug.Log("Cannot shoot - no weapon or out of ammo");
            }
        }
    }
    
    void Update()
        {
            if (agent == null) return;

            HandleMovement();
            HandleShooting();
            ToggleCursorLock();
        }

        void Shoot()
    {
        if (!GameUI.Instance.UseWeapon()) return;

        Debug.Log("Player shooting");
        
        GameObject bullet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        bullet.name = "PlayerBullet";
        bullet.tag = "PlayerBullet";
        
        bullet.transform.position = transform.position + transform.forward * 1f + Vector3.up * 0.5f;
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
        bulletComponent.bulletType = Bullet.BulletType.Player;
        
        Destroy(bullet, 2f);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Health"))
        {
            Destroy(other.gameObject);
            itemSpawner.RespawnItem(itemSpawner.healthPrefab, itemSpawner.spawnedHealthItems);
        }
        else if (other.CompareTag("Armor"))
        {
            Destroy(other.gameObject);
            itemSpawner.RespawnItem(itemSpawner.armorPrefab, itemSpawner.spawnedArmorItems);
        }
        else if (other.CompareTag("Weapon"))
        {
            GameUI.Instance.PickUpWeapon();
            Debug.Log("Weapon picked up!");
            Destroy(other.gameObject);
            itemSpawner.RespawnItem(itemSpawner.weaponPrefab, itemSpawner.spawnedWeapons);
        }
        else if (other.CompareTag("NPCBullet"))
        {
            Debug.Log("Player hit by bullet!");
            GameUI.Instance.TakeDamage(20);
            Destroy(other.gameObject);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("NPCBullet"))
        {
            Debug.Log("Player hit by bullet!");
            GameUI.Instance.TakeDamage(20);
            Destroy(collision.gameObject);
        }
    }

    public void PickUpWeapon()
    {
    hasWeapon = true;
    Debug.Log($"Weapon picked up! HasWeapon: {hasWeapon}");
    }

        private void RespawnPlayer()
    {
        agent.enabled = false;
        transform.position = initialPosition;
        transform.rotation = initialRotation;
        agent.enabled = true;
        Debug.Log("Player respawned at initial position");
    }

    void OnEnable()
    {
        GameUI.OnPlayerDeath += RespawnPlayer;
    }

    void OnDisable()
    {
        GameUI.OnPlayerDeath -= RespawnPlayer;
    }
}