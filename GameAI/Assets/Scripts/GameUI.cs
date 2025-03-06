using UnityEngine;
using TMPro;
using System.Collections;

public class GameUI : MonoBehaviour 
{
    [Header("UI References")]
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI armorText;
    public TextMeshProUGUI weaponText;
    public TextMeshProUGUI pointsText;
    public TextMeshProUGUI boostText;
    public TextMeshProUGUI magicPotionText;

    public MeshRenderer playerRenderer;
    private static GameUI instance;
    public static GameUI Instance { get { return instance; } }

    private int currentHealth = 100;
    private const int MAX_HEALTH = 100;
    private const int HEALTH_INCREMENT = 50;
    private const int BULLET_DAMAGE = 20;
    private int points = 0;

    private bool hasWeapon = false;
    private const int MAX_AMMO = 10;
    private int currentAmmo;

    private bool hasArmor = false;
    private float armorTimer = 0f;
    private const float MAX_ARMOR_TIME = 5f;
    private Coroutine armorCoroutine;
    
    private bool hasBoost = false;
    private float boostTimer = 0f;
    private const float MAX_BOOST_TIME = 5f;
    private Coroutine boostCoroutine;
    private bool isInvisible = false;
    private float potionTimer = 0f;
    private const float MAX_POTION_TIME = 5f;
    private Coroutine potionCoroutine;

    public static event System.Action OnCoinPickup;
    public static event System.Action OnPlayerDeath;


    void Awake()
    {
        if (instance == null) instance = this;
    }

    void Start()
    {
        UpdateHealthUI();
        UpdateArmorUI();
        UpdateWeaponUI();
        UpdatePointsUI();
        UpdateBoostUI();
        UpdateMagicPotionUI();

        }
    public void ActivateBoost()
    {
        if (hasBoost)
        {
            ResetBoostTimer();
            return;
        }

        hasBoost = true;
        boostTimer = MAX_BOOST_TIME;
        
        if (boostCoroutine != null)
        {
            StopCoroutine(boostCoroutine);
        }

        boostCoroutine = StartCoroutine(BoostTimerCoroutine());
        UpdateBoostUI();
    }

    public bool IsPlayerInvisible()
    {
        return isInvisible;
    }

    public void ActivateMagicPotion()
    {
        if (isInvisible)
        {
            ResetPotionTimer();
            return;
        }

        isInvisible = true;
        potionTimer = MAX_POTION_TIME;
        
        if (playerRenderer != null)
        {
            playerRenderer.enabled = false;
        }
        
        if (potionCoroutine != null)
        {
            StopCoroutine(potionCoroutine);
        }

        potionCoroutine = StartCoroutine(PotionTimerCoroutine());
        UpdateMagicPotionUI();
    }

    private IEnumerator PotionTimerCoroutine()
    {
        while (potionTimer > 0)
        {
            yield return new WaitForSeconds(0.1f);
            potionTimer -= 0.1f;
            UpdateMagicPotionUI();
        }

        isInvisible = false;
        
        if (playerRenderer != null)
        {
            playerRenderer.enabled = true;
        }
        UpdateMagicPotionUI();
    }

    private void ResetPotionTimer()
    {
        if (potionCoroutine != null)
        {
            StopCoroutine(potionCoroutine);
        }

        potionTimer = MAX_POTION_TIME;
        isInvisible = true;
        potionCoroutine = StartCoroutine(PotionTimerCoroutine());
        UpdateMagicPotionUI();
    }

    private void UpdateMagicPotionUI()
    {
        if (magicPotionText == null) return;

        if (!isInvisible)
        {
            magicPotionText.text = "Magic: false";
        }
        else
        {
            magicPotionText.text = $"Magic: {potionTimer:F1}s";
        }
    }

    private IEnumerator BoostTimerCoroutine()
    {
        while (boostTimer > 0)
        {
            yield return new WaitForSeconds(0.1f);
            boostTimer -= 0.1f;
            UpdateBoostUI();
        }

        hasBoost = false;
        UpdateBoostUI();
    }

    private void ResetBoostTimer()
    {
        if (boostCoroutine != null)
        {
            StopCoroutine(boostCoroutine);
        }

        boostTimer = MAX_BOOST_TIME;
        hasBoost = true;
        boostCoroutine = StartCoroutine(BoostTimerCoroutine());
        UpdateBoostUI();
    }

    private void UpdateBoostUI()
    {
        if (boostText == null) return;

        if (!hasBoost)
        {
            boostText.text = "Boost: false";
        }
        else
        {
            boostText.text = $"Boost: {boostTimer:F1}s";
        }
    }
        
    public bool HasBoost()
    {
        return hasBoost;
    }
    public void AddPoint()
    {
        points++;
        UpdatePointsUI();
        OnCoinPickup?.Invoke();
        Debug.Log($"Point added. Total points: {points}, triggering NPC alert");
    }

    private void UpdatePointsUI()
    {
        if (pointsText != null)
        {
            pointsText.text = $"Points: {points}";
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth = Mathf.Max(0, currentHealth - damage);
        UpdateHealthUI();
        
        if (currentHealth <= 0)
        {
            OnPlayerDeath?.Invoke();
            currentHealth = 100;
            UpdateHealthUI();
        }
    }

    public void PickUpArmor()
    {
        if (hasArmor)
        {
            ResetArmorTimer();
            return;
        }

        hasArmor = true;
        armorTimer = MAX_ARMOR_TIME;
        
        if (armorCoroutine != null)
        {
            StopCoroutine(armorCoroutine);
        }

        armorCoroutine = StartCoroutine(ArmorTimerCoroutine());
        UpdateArmorUI();
    }

    private IEnumerator ArmorTimerCoroutine()
    {
        while (armorTimer > 0)
        {
            yield return new WaitForSeconds(0.1f);
            armorTimer -= 0.1f;
            UpdateArmorUI();
        }

        hasArmor = false;
        UpdateArmorUI();
    }

    private void ResetArmorTimer()
    {
        if (armorCoroutine != null)
        {
            StopCoroutine(armorCoroutine);
        }

        armorTimer = MAX_ARMOR_TIME;
        hasArmor = true;
        armorCoroutine = StartCoroutine(ArmorTimerCoroutine());
        UpdateArmorUI();
    }

    private void UpdateArmorUI()
    {
        if (armorText == null) return;

        if (!hasArmor)
        {
            armorText.text = "Armor: false";
        }
        else
        {
            armorText.text = $"Armor: {armorTimer:F1}s";
        }
    }

    public void AddHealth()
    {
        currentHealth = Mathf.Min(currentHealth + HEALTH_INCREMENT, MAX_HEALTH);
        UpdateHealthUI();
    }

    private void UpdateHealthUI()
    {
        healthText.text = $"Health: {currentHealth}";
    }

    public bool UseWeapon()
    {
        Debug.Log($"UseWeapon called - hasWeapon: {hasWeapon}, currentAmmo: {currentAmmo}");

        if (!hasWeapon || currentAmmo <= 0)
        {
            hasWeapon = false;
            UpdateWeaponUI();
            Debug.Log("Cannot use weapon - no weapon or out of ammo");
            return false;
        }

        currentAmmo--;
        UpdateWeaponUI();
        Debug.Log($"Weapon used. Remaining ammo: {currentAmmo}");
        return true;
    }

    public void PickUpWeapon()
    {
        hasWeapon = true;
        currentAmmo = MAX_AMMO;
        UpdateWeaponUI();
        Debug.Log($"Weapon picked up. Ammo set to {currentAmmo}");
    }

    private void UpdateWeaponUI()
    {
        if (weaponText == null)
        {
            Debug.LogError("Weapon Text is not assigned!");
            return;
        }

        if (!hasWeapon)
        {
            weaponText.text = "Weapon: false";
        }
        else
        {
            weaponText.text = $"Weapon: {currentAmmo}/{MAX_AMMO}";
        }
        Debug.Log($"Weapon UI Updated: {weaponText.text}");
    }

}
