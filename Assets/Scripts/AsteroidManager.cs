using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AsteroidManager : MonoBehaviour
{
    [Header("Asteroid Settings")]
    public GameObject[] rock;              // Asteroid prefabs
    public Transform player;
    public float minSpawnDistance = 1000f;
    public float maxSpawnDistance = 1100f;
    public float horizontalSpread = 70f;
    public float verticalSpread = 60f;
    public bool canSpawn = true;

    [Header("Pooling Settings")]
    public int poolSize = 60;

    // ----------------- Asteroid Data -----------------
    private class AsteroidData
    {
        public GameObject obj;
        public Transform tf;
        public Vector3 rotationAxis;
        public float rotationSpeed;
        public float moveSpeed;
    }
    private List<AsteroidData> pool = new List<AsteroidData>();

    [Header("Timing Settings")]
    public float initialDelay = 2f;
    public float spawnInterval = 0.1f;
    private float spawnTimer;

    [Header("Difficulty Settings")]
    public int baseAsteroidsPerSpawn = 2;
    public int maxAsteroidsPerSpawn = 10;
    public float difficultyIncreaseInterval = 10f;
    private float difficultyTimer;
    private int currentAsteroidsPerSpawn;

    [Header("Destroy Effect")]
    public ParticleSystem destroyEffectPrefab;
    public int effectPoolSize = 20;
    private Queue<ParticleSystem> effectPool = new Queue<ParticleSystem>();

    // ----------------- Initialization -----------------
    void Start()
    {
        if (rock == null || rock.Length == 0 || player == null)
        {
            Debug.LogError("Asteroids or player not set!");
            enabled = false;
            return;
        }

        InitializeAsteroidPool();
        InitializeEffectPool();

        spawnTimer = initialDelay;
        difficultyTimer = difficultyIncreaseInterval;
        currentAsteroidsPerSpawn = baseAsteroidsPerSpawn;
    }

    void InitializeAsteroidPool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject prefab = rock[Random.Range(0, rock.Length)];
            GameObject asteroid = Instantiate(prefab, transform);
            asteroid.SetActive(false);

            // Add trigger collider if missing
            Collider col = asteroid.GetComponent<Collider>();
            if (col == null) col = asteroid.AddComponent<SphereCollider>();
            col.isTrigger = true;

            // Random movement/rotation
            Vector3 rotAxis = Random.onUnitSphere;
            float rotSpeed = Random.Range(30f, 90f);
            float moveSpeed = Random.Range(5f, 15f);

            // Add internal trigger script for bullet collisions
            if (asteroid.GetComponent<BulletTrigger>() == null)
                asteroid.AddComponent<BulletTrigger>().manager = this;

            pool.Add(new AsteroidData
            {
                obj = asteroid,
                tf = asteroid.transform,
                rotationAxis = rotAxis,
                rotationSpeed = rotSpeed,
                moveSpeed = moveSpeed
            });
        }
    }

    void InitializeEffectPool()
    {
        if (destroyEffectPrefab == null) return;
        for (int i = 0; i < effectPoolSize; i++)
        {
            ParticleSystem ps = Instantiate(destroyEffectPrefab);
            ps.gameObject.SetActive(false);
            effectPool.Enqueue(ps);
        }
    }

    // ----------------- Update -----------------
    void Update()
    {
        if (!canSpawn || player == null) return;

        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f)
        {
            SpawnFromPool();
            spawnTimer = spawnInterval;
        }

        difficultyTimer -= Time.deltaTime;
        if (difficultyTimer <= 0f)
        {
            IncreaseDifficulty();
            difficultyTimer = difficultyIncreaseInterval;
        }

        MoveAsteroids();
    }

    // ----------------- Spawning -----------------
    void SpawnFromPool()
    {
        int spawned = 0;
        foreach (var data in pool)
        {
            if (spawned >= currentAsteroidsPerSpawn) break;
            if (!data.obj.activeSelf)
            {
                float distance = Random.Range(minSpawnDistance, maxSpawnDistance);
                float xOffset = Random.Range(-horizontalSpread, horizontalSpread);
                float yOffset = Random.Range(-verticalSpread, verticalSpread);

                Vector3 spawnPos = player.position +
                                   (player.forward * distance) +
                                   (player.right * xOffset) +
                                   (player.up * yOffset);

                data.tf.position = spawnPos;
                data.tf.rotation = Random.rotation;
                data.obj.SetActive(true);
                spawned++;
            }
        }
    }

    void MoveAsteroids()
    {
        foreach (var data in pool)
        {
            if (!data.obj.activeSelf) continue;

            data.tf.Rotate(data.rotationAxis * data.rotationSpeed * Time.deltaTime, Space.World);
            data.tf.Translate(Vector3.back * data.moveSpeed * Time.deltaTime, Space.World);

            Vector3 toAsteroid = data.tf.position - player.position;
            if (Vector3.Dot(player.forward, toAsteroid) < -100f) // despawn
                data.obj.SetActive(false);
        }
    }

    void IncreaseDifficulty()
    {
        if (currentAsteroidsPerSpawn < maxAsteroidsPerSpawn)
            currentAsteroidsPerSpawn++;
    }

    // ----------------- Destroy Asteroid -----------------
    public void DestroyAsteroid(GameObject asteroid)
    {
        if (destroyEffectPrefab != null && effectPool.Count > 0)
        {
            ParticleSystem ps = effectPool.Dequeue();
            ps.transform.position = asteroid.transform.position;
            ps.transform.rotation = Quaternion.identity;
            ps.gameObject.SetActive(true);
            ps.Play();
            StartCoroutine(ReturnEffectToPool(ps));
        }

        asteroid.SetActive(false);
    }

    private IEnumerator ReturnEffectToPool(ParticleSystem ps)
    {
        yield return new WaitForSeconds(ps.main.duration);
        ps.gameObject.SetActive(false);
        effectPool.Enqueue(ps);
    }

    // ----------------- Bullet Trigger -----------------
    private class BulletTrigger : MonoBehaviour
    {
        [HideInInspector] public AsteroidManager manager;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Bullet"))
            {
                manager.DestroyAsteroid(gameObject);
                other.gameObject.SetActive(false);
            }
        }
    }
}
