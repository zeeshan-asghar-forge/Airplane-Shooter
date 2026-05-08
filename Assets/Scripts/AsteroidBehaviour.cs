using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AsteroidBehaviour : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeedMin = 5f;
    public float moveSpeedMax = 15f;
    private float moveSpeed;

    [Header("Cinematic Speed Increase")]
    public float speedMultiplier = 1.2f;
    public float increaseInterval = 20f;
    private static float globalSpeedFactor = 1f;

    [Header("Rotation Settings")]
    private Vector3 randomRotation;
    private float rotationSpeed;

    [Header("Despawn Settings")]
    public Transform player;
    public float despawnDistance = 100f;

    [Header("Destroy Effect")]
    public ParticleSystem destroyEffectPrefab;

    private static bool speedRoutineStarted = false;

    // -------- Particle Pool (Shared) --------
    private static Queue<ParticleSystem> effectPool = new Queue<ParticleSystem>();
    private const int poolSize = 20;

    private void OnEnable()
    {
        moveSpeed = Random.Range(moveSpeedMin, moveSpeedMax);

        randomRotation = new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f)
        ).normalized;

        rotationSpeed = Random.Range(30f, 90f);

        if (!speedRoutineStarted)
        {
            speedRoutineStarted = true;
            StartCoroutine(IncreaseSpeedOverTime());
        }

        InitializePool();
    }

    private void InitializePool()
    {
        if (destroyEffectPrefab == null || effectPool.Count > 0)
            return;

        for (int i = 0; i < poolSize; i++)
        {
            ParticleSystem ps = Instantiate(destroyEffectPrefab);
            ps.gameObject.SetActive(false);
            effectPool.Enqueue(ps);
        }
    }

    private IEnumerator IncreaseSpeedOverTime()
    {
        while (true)
        {
            yield return new WaitForSeconds(increaseInterval);
            globalSpeedFactor *= speedMultiplier;
        }
    }

    void Update()
    {
        transform.Rotate(randomRotation * rotationSpeed * Time.deltaTime);

        float finalSpeed = moveSpeed * globalSpeedFactor;
        transform.Translate(Vector3.back * finalSpeed * Time.deltaTime, Space.World);

        if (player != null)
        {
            Vector3 toAsteroid = transform.position - player.position;
            float dot = Vector3.Dot(player.forward, toAsteroid);

            if (dot < -despawnDistance)
            {
                gameObject.SetActive(false);
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        PlayDestroyEffect();
        gameObject.SetActive(false);
    }

    private void PlayDestroyEffect()
    {
        if (effectPool.Count == 0)
            return;

        ParticleSystem ps = effectPool.Dequeue();
        ps.transform.position = transform.position;
        ps.transform.rotation = Quaternion.identity;

        ps.gameObject.SetActive(true);
        ps.Play();

        StartCoroutine(ReturnEffectToPool(ps));
    }

    private IEnumerator ReturnEffectToPool(ParticleSystem ps)
    {
        yield return new WaitForSeconds(ps.main.duration);
        ps.gameObject.SetActive(false);
        effectPool.Enqueue(ps);
    }
}
