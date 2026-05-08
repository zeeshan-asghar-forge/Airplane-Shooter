using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LobbyUIDisplay : MonoBehaviour
{
    public string HighScoreName = "HighScoreBasic";
    private int HighScore;
    public TextMeshProUGUI HighScoreText;


    void Awake()
    {
        HighScore = PlayerPrefs.GetInt(HighScoreName, 0);
        ShowUserStatic();
    }

    public void ShowUserStatic()
    {
        HighScoreText.text = HighScore.ToString("N0");
    }

}
