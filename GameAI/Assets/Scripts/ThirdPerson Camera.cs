using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Transform target;
    
    [Header("Camera Settings")]
    [SerializeField] private float distance = 5f;
    [SerializeField] private float height = 2f;
    [SerializeField] private float smoothSpeed = 10f;
    
    [Header("Collision Settings")]
    [SerializeField] private float minDistance = 1f;
    [SerializeField] private LayerMask collisionLayers;
    
    private Vector3 targetPosition;
    private Vector3 smoothVelocity = Vector3.zero;
    
    void Start()
    {
        if (target == null)
        {
            Debug.LogError("No target assigned to ThirdPersonCamera!");
            enabled = false;
            return;
        }
        
        PositionCamera();
    }
    
    void LateUpdate()
    {
        if (target == null) return;
        
        PositionCamera();
    }
    
    void PositionCamera()
    {
        Vector3 desiredPosition = target.position - target.forward * distance + Vector3.up * height;
        
        RaycastHit hit;
        if (Physics.Linecast(target.position + Vector3.up * height, desiredPosition, out hit, collisionLayers))
        {
            desiredPosition = hit.point + hit.normal * 0.2f;
        }
        
        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref smoothVelocity,
            1f / smoothSpeed
        );
        
        Vector3 lookPosition = target.position + Vector3.up * height * 0.5f;
        transform.LookAt(lookPosition);
    }
}