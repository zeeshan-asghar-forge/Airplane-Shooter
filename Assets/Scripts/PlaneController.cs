using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlaneController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float forwardSpeed = 10f;
    public float boostSpeed = 20f;
    public float ascendSpeed = 5f;
    public float descendSpeed = 5f;
    public float yawSpeed = 50f;

    [Header("Auto Speed Increase")]
    public float speedMultiplier = 1.2f;
    public float increaseInterval = 20f;

    [Header("Rotation Settings")]
    public float tiltAmount = 30f;
    public float tiltSmooth = 5f;
    public float pitchAmount = 20f;
    public float pitchSmooth = 5f;
    public float resetSmooth = 3f;

    [Header("Respawn Settings")]
    public Transform respawnPoint;
    public float respawnDelay = 1.5f;
    public bool autoRespawn = true;
    public KeyCode respawnKey = KeyCode.R;

    [Header("Input Settings")]
    public float mouseSensitivity = 2.5f;
    public float touchSensitivity = 0.015f;
    public float inputSmoothing = 12f;

    [Header("Shooting")]
    public Shooter[] weapons;

    private int currentHealth;
    private Rigidbody rb;
    private Collider planeCollider;
    private float currentSpeed;
    private bool isBoosting = false;
    private bool engineOn = true;
    private bool isDead = false;

    private Vector2 rawInput;
    private Vector2 smoothInput;
    private Quaternion targetRotation;

    private LevelManager levelManager;

    [HideInInspector]
    public bool shootPressed = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        planeCollider = GetComponent<Collider>();
        levelManager = Object.FindFirstObjectByType<LevelManager>();

        rb.useGravity = false;
        rb.isKinematic = false;

        currentSpeed = forwardSpeed;
        targetRotation = transform.rotation;

        if (levelManager)
        {
            levelManager.SetupHealth();
            currentHealth = levelManager.maxHealth;
        }
    }

    private void Update()
    {
        if (!engineOn || isDead)
        {
            if (!autoRespawn && Input.GetKeyDown(respawnKey))
                RespawnPlane();
            return;
        }

        ReadInput();
        HandleShooting();

        smoothInput = Vector2.Lerp(smoothInput, rawInput, Time.deltaTime * inputSmoothing);

        transform.Translate(Vector3.forward * currentSpeed * Time.deltaTime, Space.Self);
        transform.Translate(new Vector3(smoothInput.x, smoothInput.y, 0f), Space.World);

        currentSpeed = isBoosting ? boostSpeed : forwardSpeed;

        ApplySmoothRotation();
    }

    private void ReadInput()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        rawInput.x = Input.GetAxisRaw("Mouse X") * mouseSensitivity;
        rawInput.y = Input.GetAxisRaw("Mouse Y") * mouseSensitivity;
        isBoosting = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
#else
        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);
            rawInput = t.deltaPosition * touchSensitivity;
        }
        else
        {
            rawInput = Vector2.zero;
        }
        isBoosting = false;
#endif
    }

    private void HandleShooting()
    {
        if (weapons == null || weapons.Length == 0) return;

#if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetMouseButton(0)) FireAllWeapons();
#else
        if (shootPressed) FireAllWeapons();
#endif
    }

    private void FireAllWeapons()
    {
        for (int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i] != null) weapons[i].Fire();
        }
    }

    private void ApplySmoothRotation()
    {
        float targetTilt = -smoothInput.x * tiltAmount;
        float targetPitch = smoothInput.y * pitchAmount;

        Quaternion yawRotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);
        Quaternion tiltRotation = Quaternion.Euler(0, 0, targetTilt);
        Quaternion pitchRotation = Quaternion.Euler(targetPitch, 0, 0);

        targetRotation = yawRotation * pitchRotation * tiltRotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * tiltSmooth);
    }

    public void TurnEngineOn()
    {
        engineOn = true;
        isDead = false;
        rb.useGravity = false;
        rb.linearVelocity = Vector3.zero;
    }

    public void TurnEngineOff()
    {
        engineOn = false;
        rb.useGravity = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Asteroid"))
        {
            int damage = levelManager != null ? levelManager.asteroidDamage : 30;
            TakeDamage(damage);
        }
    }

    private void TakeDamage(int damage)
    {
        if (isDead) return;

        int maxH = levelManager != null ? levelManager.maxHealth : 100;
        currentHealth = Mathf.Clamp(currentHealth - damage, 0, maxH);

        if (levelManager) levelManager.UpdateHealth(currentHealth);

        if (currentHealth <= 0)
            StartCoroutine(HandleCrash());
    }

    private IEnumerator HandleCrash()
    {
        isDead = true;
        TurnEngineOff();

        if (autoRespawn)
        {
            yield return new WaitForSeconds(respawnDelay);
            RespawnPlane();
        }
    }

    public void RespawnPlane()
    {
        if (!respawnPoint) return;
        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        rb.useGravity = false;
        rb.isKinematic = true;
        planeCollider.enabled = false;

        transform.position = respawnPoint.position;
        transform.rotation = respawnPoint.rotation;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        yield return new WaitForSeconds(0.5f);

        planeCollider.enabled = true;
        rb.isKinematic = false;

        if (levelManager)
        {
            currentHealth = levelManager.maxHealth;
            levelManager.UpdateHealth(currentHealth);
        }

        TurnEngineOn();
    }

    public void ShootButtonDown() => shootPressed = true;
    public void ShootButtonUp() => shootPressed = false;
    public void BoostButtonDown() => isBoosting = true;
    public void BoostButtonUp() => isBoosting = false;
}