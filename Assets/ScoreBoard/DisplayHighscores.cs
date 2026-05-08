// ------------------------------------------------------
// ✅ DisplayHighscores.cs (Final - Hide #ID in names)
// ------------------------------------------------------
// Works with Unity Gaming Services Leaderboards (UGS)
// Automatically removes the "#xxxx" part from usernames
// ------------------------------------------------------

using System.Collections;
using UnityEngine;
using TMPro;

public class DisplayHighscores : MonoBehaviour
{
    public TextMeshProUGUI[] rNames;   // Player names UI
    public TextMeshProUGUI[] rScores;  // Scores UI

    public LeaderboardType leaderboardType;  // Stars / HighestScore

    private HighScores myScores;

    void Start()
    {
        // Initialize UI placeholders
        for (int i = 0; i < rNames.Length; i++)
        {
            rNames[i].text = (i + 1) + ". Fetching...";
            rScores[i].text = "";
        }

        myScores = GetComponent<HighScores>();

        // Start periodic refresh
        StartCoroutine(RefreshHighscores());
    }

    /// <summary>
    /// Called by HighScores.cs after fetching leaderboard data.
    /// </summary>
    public void SetScoresToMenu(PlayerScore[] highscoreList)
    {
        for (int i = 0; i < rNames.Length; i++)
        {
            rNames[i].text = "-";
            rScores[i].text = "";

            if (highscoreList != null && i < highscoreList.Length)
            {
                string displayName = highscoreList[i].username;

                // ✅ Remove Unity's unique suffix like "Dev#9871"
                if (displayName.Contains("#"))
                    displayName = displayName.Split('#')[0];

                rNames[i].text = displayName;
                rScores[i].text = highscoreList[i].score.ToString("N0");
            }
        }
    }

    /// <summary>
    /// Refresh leaderboard every 30 seconds.
    /// </summary>
    IEnumerator RefreshHighscores()
    {
        while (true)
        {
            if (myScores != null)
            {
                HighScores.DownloadScores(leaderboardType);
            }
            yield return new WaitForSeconds(30f);
        }
    }
}
