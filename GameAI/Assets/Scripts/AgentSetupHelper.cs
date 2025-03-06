using UnityEngine;
using UnityEngine.AI;

public class AgentSetupHelper : MonoBehaviour
{
    [SerializeField] private float heightOffset = 1.0f;
    
    void Start()
    {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position, out hit, 10f, NavMesh.AllAreas))
        {
            transform.position = hit.position + Vector3.up * heightOffset;
            Debug.Log("Agent positioned on NavMesh at: " + transform.position);
        }
        else
        {
            Debug.LogError("Could not find valid NavMesh position!");
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * 2f);
    }
}
