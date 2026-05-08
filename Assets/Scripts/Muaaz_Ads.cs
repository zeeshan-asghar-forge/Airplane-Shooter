// ------------------------------------------------------
// ✅ Muaaz_Ads.cs (WebGL - CrazyGames SDK with Midgame + Rewarded Ads)
// ------------------------------------------------------
// • Uses official CrazyGames SDK (no JS fallback needed)
// • Interstitials use CrazyAdType.Midgame (correct enum)
// • Keeps rewardedAdReady flag for other scripts
// • Safe for Unity Editor & WebGL builds
// ------------------------------------------------------

using UnityEngine;
using System;
using CrazyGames; // Official CrazyGames SDK namespace

public class Muaaz_Ads : MonoBehaviour
{
    public static Muaaz_Ads Instance;

    // ✅ Event: Notifies other scripts (like Revive) when rewarded ad completes
    public Action OnRewardedAdCompleted;

    [Header("CrazyGames Ads Settings")]
    private bool interstitialAdReady = true; // CrazyGames SDK handles ads instantly
    public bool rewardedAdReady = false; // For other scripts to check if ready
    private bool crazyInitialized = false;

    void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeCrazySDK();

        Debug.LogWarning("[Muaaz_Ads] CrazyGames Ads only work in WebGL builds.");
    }

    // ------------------------------------------------------
    // ✅ Initialize CrazyGames SDK
    // ------------------------------------------------------
    private void InitializeCrazySDK()
    {
        if (CrazySDK.IsAvailable)
        {
            try
            {
                CrazySDK.Init(() =>
                {
                    crazyInitialized = true;
                    interstitialAdReady = true;
                    rewardedAdReady = true;
                    Debug.Log("[Muaaz_Ads] CrazySDK initialized successfully.");
                });
            }
            catch (Exception e)
            {
                Debug.LogError("[Muaaz_Ads] CrazySDK Init failed: " + e.Message);
            }
        }
        else
        {
            Debug.LogWarning("[Muaaz_Ads] CrazySDK not available at runtime.");
        }
    }

    // ------------------------------------------------------
    // 🎥 Show Interstitial (Midgame) Ad
    // ------------------------------------------------------
    public void ShowInterstitialAd()
    {
        if (!crazyInitialized)
        {
            Debug.LogWarning("[Muaaz_Ads] CrazySDK not initialized yet.");
            DirectLevelLoad(); // fallback if ad not ready
            return;
        }

        if (!interstitialAdReady)
        {
            Debug.Log("[Muaaz_Ads] Interstitial not ready, skipping ad.");
            DirectLevelLoad();
            return;
        }

        interstitialAdReady = false;
        Debug.Log("[Muaaz_Ads] Requesting CrazyGames Midgame Ad...");

        CrazySDK.Ad.RequestAd(
            CrazyAdType.Midgame,
            adStarted: () =>
            {
                Debug.Log("[Muaaz_Ads] Interstitial ad started.");
                Time.timeScale = 0f;
                AudioListener.pause = true;
            },
            adError: (err) =>
            {
                Debug.LogError("[Muaaz_Ads] Interstitial ad error: " + err);
                Time.timeScale = 1f;
                AudioListener.pause = false;
                interstitialAdReady = true;
                DirectLevelLoad(); // fallback on error
            },
            adFinished: () =>
            {
                Debug.Log("[Muaaz_Ads] Interstitial ad finished.");
                Time.timeScale = 1f;
                AudioListener.pause = false;
                interstitialAdReady = true;

                PlayerPrefs.SetInt("AdCount", 0); // same as AdMob version
                DirectLevelLoad();
            }
        );
    }

    // ------------------------------------------------------
    // ✅ Load Level (like AdMob's OnAdClosed)
    // ------------------------------------------------------
    private void DirectLevelLoad()
    {
        ShowInterstitialCaller[] callers = FindObjectsOfType<ShowInterstitialCaller>();
        foreach (var caller in callers)
        {
            if (caller.ButtonClicked)
            {
                caller.LoadLevel();
                break;
            }
        }
    }

    // ------------------------------------------------------
    // 🎁 Show Rewarded Ad (grants a reward)
    // ------------------------------------------------------
    public void ShowRewardedAd()
    {
        if (!crazyInitialized)
        {
            Debug.LogWarning("[Muaaz_Ads] CrazySDK not initialized yet.");
            return;
        }

        rewardedAdReady = false;
        Debug.Log("[Muaaz_Ads] Requesting Rewarded Ad...");

        CrazySDK.Ad.RequestAd(
            CrazyAdType.Rewarded,
            adStarted: () =>
            {
                Debug.Log("[Muaaz_Ads] Rewarded ad started.");
                Time.timeScale = 0f;
                AudioListener.pause = true;
            },
            adError: (err) =>
            {
                Debug.LogError("[Muaaz_Ads] Rewarded ad error: " + err);
                Time.timeScale = 1f;
                AudioListener.pause = false;
                rewardedAdReady = true;
            },
            adFinished: () =>
            {
                Debug.Log("[Muaaz_Ads] Rewarded ad finished — reward granted.");
                Time.timeScale = 1f;
                AudioListener.pause = false;
                GrantReward();
                rewardedAdReady = true;

                // ✅ Notify Revive.cs that rewarded ad completed successfully
                OnRewardedAdCompleted?.Invoke();
            }
        );
    }

    // ------------------------------------------------------
    // ✅ Grant Reward to Player
    // ------------------------------------------------------
    private void GrantReward()
    {
        // Example for your second project (uses PlayerHealth.Revive)
        PlayerHealth levelScript = FindAnyObjectByType<PlayerHealth>();

        if (levelScript != null)
        {
            levelScript.Revive();
            Debug.Log("[Muaaz_Ads] Player rewarded (extra life / resume).");
        }
        else
        {
            Debug.LogWarning("[Muaaz_Ads] PlayerHealth not found — reward skipped.");
        }
    }
}
