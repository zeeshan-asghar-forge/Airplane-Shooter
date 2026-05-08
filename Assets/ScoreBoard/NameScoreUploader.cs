// ------------------------------------------------------
// ? NameScoreUploader.cs (Final - UGS Integrated)
// ------------------------------------------------------
// Works with Unity Gaming Services Leaderboards
// Updates Unity Authentication DisplayName after saving
// ------------------------------------------------------

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Services.Authentication; // ? Required for UpdatePlayerNameAsync

public class NameScoreUploader : MonoBehaviour
{
    [Header("UI References")]
    public GameObject setUserNamePanel;
    public TMP_InputField userNameInputField;
    public Button saveUserNameButton;
    public TextMeshProUGUI feedbackText;

    private const string userNameKey = "UserName";

    private const string HighestScorer = "Highscore"; // Score leaderboard

    void Start()
    {
        // Check if username already saved
        if (!PlayerPrefs.HasKey(userNameKey))
        {
            ShowUserNamePanel();
        }
        else
        {
            setUserNamePanel.SetActive(false);
        }

        userNameInputField.onValueChanged.AddListener(delegate { CleanUserInput(); });
        saveUserNameButton.onClick.AddListener(TrySaveUserName);
    }

    void ShowUserNamePanel()
    {
        setUserNamePanel.SetActive(true);
        feedbackText.text = "Please enter a name!";
    }

    public void TrySaveUserName()
    {
        string inputName = userNameInputField.text.Trim();

        if (string.IsNullOrEmpty(inputName))
        {
            feedbackText.text = "Please enter a name.";
            return;
        }

        if (inputName.Length > 10)
        {
            inputName = inputName.Substring(0, 10);
        }

        feedbackText.text = "Checking name...";

        // ? Check against Stars leaderboard for name uniqueness
        HighScores.CheckUsernameExists(inputName, async (exists) =>
        {
            if (exists)
            {
                feedbackText.text = "Username already taken!";
            }
            else
            {
                PlayerPrefs.SetString(userNameKey, inputName);
                PlayerPrefs.Save();

                feedbackText.text = "Username saved!";
                setUserNamePanel.SetActive(false);

                // ? Update Unity Authentication display name
                if (AuthenticationService.Instance.IsSignedIn)
                {
                    try
                    {
                        await AuthenticationService.Instance.UpdatePlayerNameAsync(inputName);
                        Debug.Log($"[NameScoreUploader] Display name updated to {inputName}");
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning("[NameScoreUploader] Failed to update display name: " + e.Message);
                    }
                }
                else
                {
                    Debug.LogWarning("[NameScoreUploader] Not signed in — display name not updated yet.");
                }
            }
        }, LeaderboardType.HighestScore);
    }

    /// <summary>
    /// Uploads scores to both leaderboards using PlayerPrefs values.
    /// </summary>
    public void UploadScoreToAllLeaderboards()
    {
        if (!PlayerPrefs.HasKey(userNameKey))
        {
            Debug.LogWarning("[NameScoreUploader] Username not set.");
            ShowUserNamePanel();
            return;
        }

        string username = PlayerPrefs.GetString(userNameKey);
        int score = PlayerPrefs.GetInt(HighestScorer, 0);

        Debug.Log($"[NameScoreUploader] Uploading {username}: Score={score}");

        HighScores.UploadScore(username, score, LeaderboardType.HighestScore);
    }

    void CleanUserInput()
    {
        string cleaned = userNameInputField.text.Replace(" ", "");
        if (cleaned != userNameInputField.text)
        {
            userNameInputField.text = cleaned;
        }
    }
}
