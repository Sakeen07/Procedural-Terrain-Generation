using UnityEngine;
using TMPro;


public class NPCUI : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI idText;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI statusText;
    
    [Header("Settings")]
    public Vector3 offset = new Vector3(0, 2.5f, 0);
    public Transform target;

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
        ValidateComponents();
    }

    void ValidateComponents()
    {
        if (idText == null) Debug.LogError("ID Text is missing!");
        if (healthText == null) Debug.LogError("Health Text is missing!");
        if (statusText == null) Debug.LogError("Status Text is missing!");
    }

    void LateUpdate()
    {
        if (target != null)
        {
            transform.position = target.position + offset;
            transform.rotation = mainCamera.transform.rotation;
        }
    }

    public void UpdateStatus(string status)
    {
        if (statusText != null)
        {
            statusText.text = $"Status: {status}";
            Debug.Log($"Updated UI status to: {status}");
        }
        else
        {
            Debug.LogError("Status Text component is null!");
        }
    }

    public void UpdateUI(string id, int health, string status)
    {
        if (idText != null) idText.text = $"ID: {id}";
        if (healthText != null) healthText.text = $"Health: {health}";
        if (statusText != null) statusText.text = $"Status: {status}";
        Debug.Log($"Updated all UI elements - Status: {status}");
    }

    public void UpdateHealth(int health)
    {
        if (healthText != null)
        {
            healthText.text = $"Health: {health}";
            Debug.Log($"Updated UI health to: {health}");
        }
        else
        {
            Debug.LogError("Health Text component is null!");
        }
    }
}