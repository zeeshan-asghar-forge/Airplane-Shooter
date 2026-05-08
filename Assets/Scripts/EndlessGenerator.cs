using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EndlessGenerator : MonoBehaviour
{
    [Header("Player & Chunk Settings")]
    public GameObject player;
    public GameObject chunkPrefab;
    public float spawnZ = 0.0f;
    public int chunksOnScreen = 7;   // on-screen chunks
    public float safeZone = 60f;

    [Header("Obstacle Prefabs")]
    public GameObject conePrefab;
    public GameObject jumpRampPrefab;

    [Header("Obstacle Settings")]
    public float platformWidth = 6f;

    [Header("Obstacle Heights")]
    public float coneGroundOffset = 0f;
    public float rampGroundOffset = 0f;

    [Header("Spawn Distance Ahead of Player")]
    public float obstacleSpawnDistance = 40f; // player کے آگے obstacles نہیں دکھیں گے

    [Header("X-axis Spread")]
    public float laneSpread = 0.5f; // lane کے اندر spread amount

    private List<GameObject> activeChunks = new List<GameObject>();

    // 5 lanes: Left -> Right
    private float[] lanes;

    void Start()
    {
        if (player == null) Debug.LogError("EndlessGenerator: player not assigned!");
        if (chunkPrefab == null) Debug.LogError("EndlessGenerator: chunkPrefab not assigned!");

        // 5 lanes setup
        lanes = new float[] { -platformWidth, -platformWidth / 2f, 0f, platformWidth / 2f, platformWidth };

        StartCoroutine(SpawnInitialChunks());
    }

    IEnumerator SpawnInitialChunks()
    {
        for (int i = 0; i < chunksOnScreen; i++)
        {
            SpawnChunk();
            yield return null;
        }
    }

    void Update()
    {
        if (player == null) return;

        if (player.transform.position.z + safeZone > spawnZ)
        {
            SpawnChunk();
        }

        if (activeChunks.Count > 0)
        {
            GameObject first = activeChunks[0];
            if (first != null && first.transform.position.z + chunkPrefab.transform.localScale.z < player.transform.position.z - safeZone)
            {
                Destroy(first);
                activeChunks.RemoveAt(0);
            }
        }
    }

    void SpawnChunk()
    {
        if (chunkPrefab == null) return;

        GameObject chunk = Instantiate(chunkPrefab, new Vector3(0, 0, spawnZ), Quaternion.identity);
        chunk.name = $"Chunk_{spawnZ}";
        activeChunks.Add(chunk);

        // Spawn obstacles for every chunk
        SpawnObstacles(chunk);

        spawnZ += chunkPrefab.transform.localScale.z;
    }

    void SpawnObstacles(GameObject chunk)
    {
        Vector3 chunkPos = chunk.transform.position;
        float chunkTopY = chunkPos.y + chunk.transform.localScale.y / 2f;

        List<int> availableLanes = new List<int> { 0, 1, 2, 3, 4 };

        // Full chunk Z boundaries
        float chunkStartZ = chunkPos.z - chunk.transform.localScale.z / 2f + 1f;
        float chunkEndZ = chunkPos.z + chunk.transform.localScale.z / 2f - 1f;

        // Player کے آگے spawn distance maintain کریں
        chunkStartZ = Mathf.Max(chunkStartZ, player.transform.position.z + obstacleSpawnDistance);

        // ---- Spawn 7 cones ----
        for (int i = 0; i < 7; i++)
        {
            if (conePrefab == null || availableLanes.Count == 0) break;

            int laneIndex = availableLanes[Random.Range(0, availableLanes.Count)];

            // Rotate lanes to cover multiple cones
            if (i % 5 == 0 && availableLanes.Count > 1)
                availableLanes.Remove(laneIndex);

            float obstacleZ = Random.Range(chunkStartZ, chunkEndZ);

            float spreadX = lanes[laneIndex] + Random.Range(-laneSpread, laneSpread);

            Vector3 conePos = new Vector3(
                spreadX,
                chunkTopY + conePrefab.transform.localScale.y / 2f + coneGroundOffset,
                obstacleZ
            );

            GameObject cone = Instantiate(conePrefab, conePos, Quaternion.identity);
            cone.transform.localScale = conePrefab.transform.localScale; // fix stretching
            cone.name = $"Cone_{chunk.name}_{i}";
        }

        // ---- Spawn 1 ramp ----
        if (jumpRampPrefab != null)
        {
            int rampLane = 2; // middle lane
            float rampZ = Random.Range(chunkStartZ, chunkEndZ);

            float rampX = lanes[rampLane] + Random.Range(-laneSpread, laneSpread);

            Vector3 rampPos = new Vector3(
                rampX,
                chunkTopY + jumpRampPrefab.transform.localScale.y / 2f + rampGroundOffset,
                rampZ
            );

            GameObject ramp = Instantiate(jumpRampPrefab, rampPos, Quaternion.identity);
            ramp.transform.localScale = jumpRampPrefab.transform.localScale;
            ramp.name = $"Ramp_{chunk.name}";
        }
    }
}
