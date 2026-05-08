using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target;   // Drag your Ball here
    public Vector3 offset = new Vector3(0, 6, -10); // Camera position relative to ball

    [Header("Smoothness Settings")]
    public float baseSmoothSpeed = 5f;   // Normal follow smoothness
    public float speedMultiplier = 0.2f; // Extra catch-up based on ball speed
    public float lookSmoothness = 5f;    // How smoothly camera rotates to look at ball

    private Rigidbody targetRb;

    void Start()
    {
        if (target != null)
            targetRb = target.GetComponent<Rigidbody>();
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Desired position
        Vector3 desiredPosition = target.position + offset;

        // Adjust smoothness based on ball velocity
        float extraSpeed = (targetRb != null) ? targetRb.linearVelocity.magnitude * speedMultiplier : 0f;
        float smoothSpeed = baseSmoothSpeed + extraSpeed;

        // Smooth follow
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;

        // Smoothly look at the ball
        Quaternion desiredRotation = Quaternion.LookRotation(target.position - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, lookSmoothness * Time.deltaTime);
    }
}
