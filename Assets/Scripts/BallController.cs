using UnityEngine;
using TMPro;   // TextMeshPro

[RequireComponent(typeof(Rigidbody), typeof(SphereCollider), typeof(AudioSource))]
public class BallController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float acceleration = 20f;
    public float maxSpeed = 25f;
    public bool autoMoveForward = true;
    public float horizontalSpeedMultiplier = 1f;

    [Header("Mobile Input Manager")]
    public MobileInputManager mobileInput;  // reference to MobileInputManager

    [Header("Jump Settings")]
    public float jumpForce = 10f;
    public LayerMask groundLayer;

    [Header("UI")]
    public TMP_Text scoreText;
    public int score;

    [Header("Speed Scaling")]
    public float interval = 20f;
    public float speedMultiplier = 1.2f;
    private float baseAcceleration;
    private float baseMaxSpeed;
    private const float maxCapFactor = 2.5f;
    private float timer = 0f;

    [Header("Audio Clips")]
    public AudioClip landClip;
    public AudioClip coneHitClip;

    [Header("Audio Settings")]
    public float minVolume = 0.25f;
    public float maxVolume = 1.0f;
    public float maxImpact = 15f;
    public float landingVelocityThreshold = 0.8f;

    private Rigidbody rb;
    private float ballRadius;
    private AudioSource audioSource;
    private bool wasAirborne = false;

    // Controlled from other scripts (like PlayerHealth)
    [HideInInspector] public bool isDead = false;

    private bool jumpPressedLastFrame = false; // used for single-tap detection

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.mass = 5f;
        rb.linearDamping = 0.5f;
        rb.angularDamping = 0.05f;
        rb.freezeRotation = false;
        rb.maxAngularVelocity = 11f;

        ballRadius = GetComponent<SphereCollider>().radius * transform.localScale.y;
        Physics.gravity = new Vector3(0, -20f, 0);

        baseAcceleration = acceleration;
        baseMaxSpeed = maxSpeed;

        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f; // 3D sound
    }

    void Update()
    {
        if (isDead) return;

        // Score update
        score = Mathf.FloorToInt(rb.position.z);
        if (scoreText != null)
            scoreText.text = score.ToString("N0");

        HandleMovement();
        HandleJump();

        // Dynamic speed scaling
        timer += Time.deltaTime;
        if (timer >= interval)
        {
            acceleration = Mathf.Min(acceleration * speedMultiplier, baseAcceleration * maxCapFactor);
            maxSpeed = Mathf.Min(maxSpeed * speedMultiplier, baseMaxSpeed * maxCapFactor);
            timer = 0f;
        }

        bool groundedNow = IsGrounded();
        if (!groundedNow)
            wasAirborne = true;
    }

    void HandleMovement()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        // Mobile overrides
        if (mobileInput != null)
        {
            if (mobileInput.IsLeftPressed()) moveX = -1f;
            else if (mobileInput.IsRightPressed()) moveX = 1f;
        }

        if (autoMoveForward)
            moveZ = 1f;

        Vector3 moveDir = new Vector3(moveX * horizontalSpeedMultiplier, 0, moveZ).normalized;
        rb.AddForce(moveDir * acceleration, ForceMode.Acceleration);

        // Clamp horizontal velocity
        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        if (flatVel.sqrMagnitude > maxSpeed * maxSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * maxSpeed;
            rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
        }
    }

    void HandleJump()
    {
        bool jumpInput = Input.GetKeyDown(KeyCode.Space);

        // Mobile override — trigger only once per press
        if (mobileInput != null)
        {
            bool jumpNow = mobileInput.IsJumpPressed();
            if (jumpNow && !jumpPressedLastFrame)
                jumpInput = true;
            jumpPressedLastFrame = jumpNow;
        }

        if (jumpInput && IsGrounded())
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, ballRadius + 0.1f, groundLayer, QueryTriggerInteraction.Ignore);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (isDead) return;

        // Handle player health
        var health = GetComponent<PlayerHealth>();
        if (health != null)
        {
            if (collision.gameObject.CompareTag("Obstacle"))
                health.TakeDamagePercent(health.coneDamagePercent);
            if (collision.gameObject.CompareTag("Spikes"))
                health.TakeDamagePercent(health.SpikesDamagePercent);
            if (collision.gameObject.CompareTag("Ramp"))
                health.HealPercent(health.rampHealPercent);
        }

        // Landing sound
        bool collidedWithGround = collision.gameObject.CompareTag("Chunk") || IsInLayerMask(collision.gameObject, groundLayer);
        if (collidedWithGround && wasAirborne)
        {
            float impact = collision.relativeVelocity.magnitude;
            if (impact >= landingVelocityThreshold && landClip != null)
            {
                float t = Mathf.Clamp01(impact / maxImpact);
                float volume = Mathf.Lerp(minVolume, maxVolume, t);
                audioSource.PlayOneShot(landClip, volume);
            }
            wasAirborne = false;
        }

        // Obstacle hit sound
        if (collision.gameObject.CompareTag("Obstacle") && coneHitClip != null)
        {
            float impact = collision.relativeVelocity.magnitude;
            float t = Mathf.Clamp01(impact / maxImpact);
            float volume = Mathf.Lerp(minVolume, maxVolume, t);
            audioSource.PlayOneShot(coneHitClip, volume);
        }
    }

    bool IsInLayerMask(GameObject obj, LayerMask mask)
    {
        return (mask.value & (1 << obj.layer)) != 0;
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (!Application.isPlaying)
            Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * (ballRadius + 0.1f));
    }
#endif
}
