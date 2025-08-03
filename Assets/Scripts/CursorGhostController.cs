using UnityEngine;
using System.Collections;

public class CursorGhostController : MonoBehaviour
{
    [Header("Movement Settings")]
    public Transform target; // Player position to move towards
    public float moveSpeed = 0.5f;
    public float floatSpeed = 1f;
    public float floatHeight = 0.1f;
    
    private Vector3 startPosition;
    private bool isMovingTowardsTarget = true;
    private float startYPosition;
    
    void Start()
    {
        startPosition = transform.position;
        startYPosition = transform.position.y;
        
        // If target is assigned, start moving towards it
        if (target != null)
        {
            StartCoroutine(MoveTowardsTarget());
        }
    }
    
    IEnumerator MoveTowardsTarget()
    {
        if (target == null) yield break;
        
        // Move towards target position (near player but not exactly on top)
        Vector3 targetPosition = target.position + new Vector3(0.5f, 0.5f, 0);
        float journeyLength = Vector3.Distance(startPosition, targetPosition);
        
        while (isMovingTowardsTarget)
        {
            float distCovered = (Time.time - 0f) * moveSpeed;
            float fractionOfJourney = distCovered / journeyLength;
            
            // Smooth movement with easing
            float easedFraction = 1 - Mathf.Pow(1 - fractionOfJourney, 3);
            transform.position = Vector3.Lerp(startPosition, targetPosition, easedFraction);
            
            // Check if we've reached the target
            if (fractionOfJourney >= 1f)
            {
                isMovingTowardsTarget = false;
                break;
            }
            
            yield return null;
        }
        
        // Once reached target, start floating animation
        StartCoroutine(FloatingAnimation());
    }
    
    IEnumerator FloatingAnimation()
    {
        while (true)
        {
            // Simple floating animation
            float newY = startYPosition + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
            yield return null;
        }
    }
    
    // Method to manually set target if needed
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        if (target != null && !isMovingTowardsTarget)
        {
            isMovingTowardsTarget = true;
            StartCoroutine(MoveTowardsTarget());
        }
    }
}
