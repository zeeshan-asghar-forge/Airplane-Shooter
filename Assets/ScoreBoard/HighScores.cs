// ------------------------------------------------------
// ✅ HighScores.cs (Final - Unity Gaming Services version)
// ------------------------------------------------------
// Fully replaces Dreamlo with Unity Leaderboards
// Supports multiple leaderboards (Stars, HighestScore)
// Automatically updates display name before upload
// ------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Leaderboards;
using System.Threading.Tasks;

public class HighScores : MonoBehaviour
{
    // Map LeaderboardType → Unity Dashboard Leaderboard IDs
    // ⚠️ Replace these with the actual IDs from your Unity Dashboard
    public static Dictionary<LeaderboardType, string> leaderboardIDs = new Dictionary<LeaderboardType, string>()
    {
        { LeaderboardType.HighestScore, "HighScoreLeaderboard" },
    };

    public PlayerScore[] scoreList;
    DisplayHighscores myDisplay;
    static HighScores instance;

    void Awake()
    {
        instance = this;
        myDisplay = GetComponent<DisplayHighscores>();
        InitializeUGS();
    }

    // ------------------------------------------------------
    // ✅ Initialize Unity Gaming Services
    // ------------------------------------------------------
    async void InitializeUGS()
    {
        try
        {
            await UnityServices.InitializeAsync();

            if (!AuthenticationService.Instance.IsSignedIn)
                await AuthenticationService.Instance.SignInAnonymouslyAsync();

            Debug.Log($"[HighScores] UGS initialized. PlayerID: {AuthenticationService.Instance.PlayerId}");
        }
        catch (System.Exception e)
        {
            Debug.LogError("[HighScores] UGS initialization failed: " + e);
        }
    }

    // ------------------------------------------------------
    // ✅ Upload Score (called from NameScoreUploader)
    // ------------------------------------------------------
    public static void UploadScore(string username, int score, LeaderboardType type)
    {
        instance.StartCoroutine(instance.UploadScoreRoutine(username, score, type));
    }

    IEnumerator UploadScoreRoutine(string username, int score, LeaderboardType type)
    {
        string leaderboardId = leaderboardIDs[type];
        Task uploadTask = UploadScoreToUGS(username, score, leaderboardId);
        yield return new WaitUntil(() => uploadTask.IsCompleted);

        if (uploadTask.Exception == null)
        {
            Debug.Log($"[HighScores] Score uploaded to {leaderboardId}: {username} = {score}");
            DownloadScores(type);
        }
        else
        {
            Debug.LogError("[HighScores] Upload failed: " + uploadTask.Exception);
        }
    }

    // ------------------------------------------------------
    // ✅ Actual upload to UGS Leaderboards
    // ------------------------------------------------------
    async Task UploadScoreToUGS(string username, int score, string leaderboardId)
    {
        try
        {
            // ✅ Update display name in Unity Authentication before upload
            if (AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.UpdatePlayerNameAsync(username);
            }

            await LeaderboardsService.Instance.AddPlayerScoreAsync(leaderboardId, score);
            Debug.Log($"[HighScores] Score submitted successfully: {score}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[HighScores] Upload error: {e}");
        }
    }

    // ------------------------------------------------------
    // ✅ Download top 10 scores from Leaderboard
    // ------------------------------------------------------
    public static void DownloadScores(LeaderboardType type)
    {
        instance.StartCoroutine(instance.DownloadTopScoresRoutine(type));
    }

    IEnumerator DownloadTopScoresRoutine(LeaderboardType type)
    {
        string leaderboardId = leaderboardIDs[type];
        Task<List<PlayerScore>> downloadTask = FetchTop10Scores(leaderboardId);
        yield return new WaitUntil(() => downloadTask.IsCompleted);

        if (downloadTask.Exception == null)
        {
            scoreList = downloadTask.Result.ToArray();
            myDisplay.SetScoresToMenu(scoreList);
            Debug.Log($"[HighScores] Leaderboard ({leaderboardId}) refreshed successfully.");
        }
        else
        {
            Debug.LogError("[HighScores] Download failed: " + downloadTask.Exception);
        }
    }

    // ------------------------------------------------------
    // ✅ Get top 10 entries
    // ------------------------------------------------------
    async Task<List<PlayerScore>> FetchTop10Scores(string leaderboardId)
    {
        List<PlayerScore> list = new List<PlayerScore>();

        try
        {
            var scores = await LeaderboardsService.Instance.GetScoresAsync(
                leaderboardId,
                new GetScoresOptions { Limit = 10 }
            );

            foreach (var entry in scores.Results)
            {
                string name = string.IsNullOrEmpty(entry.PlayerName)
                    ? entry.PlayerId
                    : entry.PlayerName;

                list.Add(new PlayerScore(name, (int)entry.Score));
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[HighScores] FetchTop10Scores error: {e}");
        }

        return list;
    }

    // ------------------------------------------------------
    // ✅ Username existence check (for uniqueness)
    // ------------------------------------------------------
    public static void CheckUsernameExists(string usernameToCheck, System.Action<bool> callback, LeaderboardType type)
    {
        instance.StartCoroutine(instance.CheckUsernameRoutine(usernameToCheck, callback, type));
    }

    IEnumerator CheckUsernameRoutine(string usernameToCheck, System.Action<bool> callback, LeaderboardType type)
    {
        string leaderboardId = leaderboardIDs[type];
        Task<bool> checkTask = UsernameExists(usernameToCheck, leaderboardId);
        yield return new WaitUntil(() => checkTask.IsCompleted);
        callback?.Invoke(checkTask.Result);
    }

    async Task<bool> UsernameExists(string usernameToCheck, string leaderboardId)
    {
        try
        {
            var scores = await LeaderboardsService.Instance.GetScoresAsync(
                leaderboardId,
                new GetScoresOptions { Limit = 100 }
            );

            foreach (var entry in scores.Results)
            {
                string name = string.IsNullOrEmpty(entry.PlayerName)
                    ? entry.PlayerId
                    : entry.PlayerName;

                if (name.ToLower() == usernameToCheck.ToLower())
                    return true;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("[HighScores] UsernameExists error: " + e);
        }

        return false;
    }
}

// ------------------------------------------------------
// Enums and Structs (same as before)
// ------------------------------------------------------
public enum LeaderboardType
{
    HighestScore,
}

public struct PlayerScore
{
    public string username;
    public int score;

    public PlayerScore(string _username, int _score)
    {
        username = _username;
        score = _score;
    }
}
