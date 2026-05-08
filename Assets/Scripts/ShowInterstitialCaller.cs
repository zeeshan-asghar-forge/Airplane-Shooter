//----------------------------------------------
// 
// Copyright © Muaaz Amir. All rights reserved.
// Contact: muaazamir.creativity@gmail.com
// 
// Unauthorized copying, distribution, or modification of this file,
// via any medium, is strictly prohibited.
// 
//----------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class ShowInterstitialCaller : MonoBehaviour
{
    public Muaaz_Ads muaaz_UnityAds;
    public Button ShowAdButton;
    private bool ableToShow = false;
    public string levelName;
    public bool ButtonClicked = false;

    // Start is called before the first frame update
    void Awake()
    {
        muaaz_UnityAds = FindObjectOfType<Muaaz_Ads>();
        int AdCount;
        AdCount = PlayerPrefs.GetInt("AdCount", 0);
        if (AdCount >= 5)
        {
            ableToShow = true;
        }
        ShowButtonUpdate();
    }

    private void ShowButtonUpdate()
    {
        ShowAdButton.onClick.AddListener(CallForAds);
    }

    public void CallForAds()
    {
        if (ableToShow)
        {
            if (muaaz_UnityAds != null)
            {
                ButtonClicked = true;
                muaaz_UnityAds.ShowInterstitialAd();
            }
            else { LoadLevel(); }
        }
        else
        {
            int AdCount;
            AdCount = PlayerPrefs.GetInt("AdCount", 0);
            AdCount++;
            PlayerPrefs.SetInt("AdCount", AdCount);
            LoadLevel();
        }
    }

    public void LoadLevel()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(levelName);
    }
}
