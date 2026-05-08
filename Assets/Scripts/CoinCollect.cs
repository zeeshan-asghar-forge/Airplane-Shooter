using UnityEngine;

public class CoinCollect : MonoBehaviour
{
    private CoinGameManager _gameManager;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip coinSound;
    [Range(0f, 1f)][SerializeField] private float coinVolume = 0.7f;

    private static AudioSource _audioSource;

    void Awake() // Initialize audio even if inactive
    {
        InitializeAudio();
    }

    public void Setup(CoinGameManager manager)
    {
        _gameManager = manager;
    }

    private void InitializeAudio()
    {
        if (_audioSource != null) return;

        GameObject go = new GameObject("CoinAudioSource");
        _audioSource = go.AddComponent<AudioSource>();
        _audioSource.spatialBlend = 0f;
        _audioSource.playOnAwake = false;
        DontDestroyOnLoad(go);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (coinSound != null && _audioSource != null)
        {
            _audioSource.PlayOneShot(coinSound, coinVolume);
        }

        if (_gameManager != null)
        {
            // Tell the manager this specific coin was hit
            _gameManager.CoinCollected(this.gameObject);
        }
    }
}