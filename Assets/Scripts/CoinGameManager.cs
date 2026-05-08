using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class CoinGameManager : MonoBehaviour
{
    [Header("Coin Settings")]
    public GameObject coinPrefab;
    public Transform player;

    [Header("Random Distance Settings")]
    public float minSpawnDistance = 40f;
    public float maxSpawnDistance = 80f;

    [Header("Horizontal Range")]
    public float horizontalRange = 20f;

    [Header("Wave Settings")]
    public float nextWaveDelay = 1f;
    public int poolSize = 10;

    private List<GameObject> coinPool = new List<GameObject>();
    private int coinsCollected = 0;
    private bool waveActive = false;

    void Start()
    {
        CreatePool();
        StartCoroutine(StartWave());
    }

    void CreatePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject coin = Instantiate(coinPrefab);
            coin.SetActive(false);

            // CRITICAL: Inject reference into the coin script
            if (coin.TryGetComponent(out CoinCollect collectScript))
            {
                collectScript.Setup(this);
            }

            coinPool.Add(coin);
        }
    }

    IEnumerator StartWave()
    {
        yield return new WaitForSeconds(nextWaveDelay);
        coinsCollected = 0;
        waveActive = true;
        SpawnNextCoin();
    }

    public void SpawnNextCoin()
    {
        if (!waveActive) return;

        GameObject coin = GetInactiveCoin();
        if (coin == null) return;

        float randomForwardDist = Random.Range(minSpawnDistance, maxSpawnDistance);
        float randomX = Random.Range(-horizontalRange, horizontalRange);

        Vector3 spawnPos = new Vector3(
            player.position.x + randomX,
            0.5f, // Elevated slightly so it's not in the floor
            player.position.z + randomForwardDist
        );

        coin.transform.position = spawnPos;
        coin.SetActive(true);
    }

    GameObject GetInactiveCoin()
    {
        int count = coinPool.Count;
        for (int i = 0; i < count; i++)
        {
            if (!coinPool[i].activeSelf)
                return coinPool[i];
        }
        return null;
    }

    public void CoinCollected(GameObject coin)
    {
        // Manager disables the coin to maintain control
        coin.SetActive(false);
        coinsCollected++;

        if (coinsCollected < poolSize)
        {
            SpawnNextCoin();
        }
        else
        {
            waveActive = false;
            StartCoroutine(StartWave());
        }
    }
}