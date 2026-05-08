//----------------------------------------------
// 
// Copyright © Zeeshan Asghar. All rights reserved.
// Contact: zeeshanasghar.forge@gmail.com
// 
// Unauthorized copying, distribution, or modification of this file,
// via any medium, is strictly prohibited.
// 
//----------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class Revive : MonoBehaviour
{
    public Muaaz_Ads muaaz_Ads;
    public Button ShowAdButton;

    // ✅ Track if player has already revived once
    private bool hasRevived = false;

    void Awake()
    {
        muaaz_Ads = FindObjectOfType<Muaaz_Ads>();
        checkButon();

        // ✅ Subscribe to ad completed event
        if (muaaz_Ads != null)
            muaaz_Ads.OnRewardedAdCompleted += OnAdCompleted;
    }

    void checkButon()
    {
        if (muaaz_Ads.rewardedAdReady)
        {
            ShowAdButton.interactable = true;
        }
        ShowButtonUpdate();
    }

    private void ShowButtonUpdate()
    {
        ShowAdButton.onClick.AddListener(CallForAds);
    }

    public void CallForAds()
    {
        // ✅ Only allow if ad manager exists and player hasn’t revived yet
        if (muaaz_Ads != null && !hasRevived)
        {
            // Show the rewarded ad (actual revive will happen after ad completes)
            muaaz_Ads.ShowRewardedAd();
        }
    }

    // ✅ Called when rewarded ad completes successfully
    private void OnAdCompleted()
    {
        hasRevived = true;
        ShowAdButton.interactable = false;
        ShowAdButton.gameObject.SetActive(false);
    }

    // ✅ Optional helper to reset revive status when new game starts
    public void ResetRevive()
    {
        hasRevived = false;
        ShowAdButton.gameObject.SetActive(true);
        ShowAdButton.interactable = true;
    }

    // ✅ Clean unsubscribe to avoid memory leaks
    void OnDestroy()
    {
        if (muaaz_Ads != null)
            muaaz_Ads.OnRewardedAdCompleted -= OnAdCompleted;
    }
}
