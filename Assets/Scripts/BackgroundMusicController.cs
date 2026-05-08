using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class BackgroundMusicController : MonoBehaviour
{
    [Header("Background Music Settings")]
    public AudioClip[] bgmClips;            // multiple BGM clips
    public bool loop = false;               // NOT used anymore (we auto-loop random)
    public float pitch = 1f;
    public float normalVolume = 0.6f;

    [Header("Fade Settings")]
    public float fadeDuration = 1.5f;
    public float fadedVolume = 0.002f;

    private AudioSource audioSource;
    private static BackgroundMusicController instance;
    private Coroutine fadeCoroutine;

    private bool playedOnce = false;

    void Awake()
    {
        // Singleton
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false; // we will manually loop randomly
        audioSource.volume = normalVolume;
        audioSource.pitch = pitch;

        // Preload audio
        if (bgmClips != null && bgmClips.Length > 0)
        {
            foreach (var c in bgmClips) if (c != null) c.LoadAudioData();
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Update()
    {
        // Do not play if player turned BGM OFF
        if (PlayerPrefs.GetInt("BGM_ON", 1) == 0)
        {
            if (audioSource.isPlaying)
                audioSource.Stop();
            return;
        }

        if (!audioSource.isPlaying && playedOnce)
        {
            PlayRandomBGM(); // play next random when previous ends
        }

        // first-time play
        if (!playedOnce && bgmClips != null && bgmClips.Length > 0)
        {
            PlayRandomBGM();
            playedOnce = true;
        }
    }

    private void PlayRandomBGM()
    {
        if (bgmClips == null || bgmClips.Length == 0) return;
        if (PlayerPrefs.GetInt("BGM_ON", 1) == 0) return;

        AudioClip clip = bgmClips[Random.Range(0, bgmClips.Length)];
        if (clip == null) return;

        audioSource.clip = clip;
        audioSource.time = 0f;
        audioSource.Play();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (PlayerPrefs.GetInt("BGM_ON", 1) == 0)
            return;

        if (scene.name == "Home" && playedOnce)
        {
            PlayRandomBGM();
        }
        if (scene.name == "Level 1" && playedOnce)
        {
            PlayRandomBGM();
        }
    }

    // Fade Methods (same as your original)
    public static void FadeOutMusic()
    {
        if (instance != null)
        {
            if (instance.fadeCoroutine != null)
                instance.StopCoroutine(instance.fadeCoroutine);

            instance.fadeCoroutine = instance.StartCoroutine(instance.instanceFadeOut());
        }
    }

    public static void FadeInMusic()
    {
        if (instance != null)
        {
            if (instance.fadeCoroutine != null)
                instance.StopCoroutine(instance.fadeCoroutine);

            instance.fadeCoroutine = instance.StartCoroutine(instance.instanceFadeIn());
        }
    }

    private IEnumerator instanceFadeOut()
    {
        float startVol = audioSource.volume;
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.unscaledDeltaTime;
            audioSource.volume = Mathf.Lerp(startVol, fadedVolume, time / fadeDuration);
            yield return null;
        }

        audioSource.volume = fadedVolume;
    }

    private IEnumerator instanceFadeIn()
    {
        float startVol = audioSource.volume;
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.unscaledDeltaTime;
            audioSource.volume = Mathf.Lerp(startVol, normalVolume, time / fadeDuration);
            yield return null;
        }

        audioSource.volume = normalVolume;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
