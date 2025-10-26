using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Tooltip("The target transform to follow.")]
    public Transform target;
    
    [Tooltip("How smoothly the camera follows the target.")]
    [SerializeField] private float smoothTime = 0.2f;
    
    [Tooltip("Offset from the target position.")]
    [SerializeField] private Vector3 offset = new Vector3(0, 0, -10);

    private Vector3 velocity = Vector3.zero;

    void LateUpdate()
    {
        if (target == null) return;
        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);
        
        var p = transform.position;
        if (p.y < 4) p.y = 4;
        transform.position = p;
    }

    public void SnapToTarget()
    {
        if (target == null) return;
        transform.position = target.position + offset;
    }
}
