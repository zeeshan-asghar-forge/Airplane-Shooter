using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelectManager : MonoBehaviour
{
    [Header("Assign your Level Select Canvas here")]
    public GameObject levelSelectCanvas;   // Drag your level_select canvas in Inspector

    private bool isSceneLoading = false;    // Prevent multiple clicks from loading scenes

    void Start()
    {
        // Ensure the level select canvas is initially hidden or visible depending on your design
        if (levelSelectCanvas != null)
            levelSelectCanvas.SetActive(false); // start hidden, will show on Play button click

        isSceneLoading = false; // No scene is loading at start
        Time.timeScale = 1f;    // Reset time scale in case previous scenes paused it
    }

    // Call this from HomeScene Play button
    public void ShowLevelSelect()
    {
        if (levelSelectCanvas != null)
            levelSelectCanvas.SetActive(true); // Show the canvas when Play is clicked
    }

    // Call this from level_select Back button
    public void HideLevelSelect()
    {
        if (levelSelectCanvas != null)
            levelSelectCanvas.SetActive(false); // Hide canvas and return to Home UI
    }

    // Call this from Stadium Level button
    public void LoadStadium()
    {
        LoadLevel("MainGame"); // Make sure scene name matches exactly in Build Settings
    }

    // Call this from Road Level button
    public void LoadRoad()
    {
        LoadLevel("Road"); // Make sure scene name matches exactly in Build Settings
    }

    // Internal method to handle scene loading safely
    private void LoadLevel(string sceneName)
    {
        if (isSceneLoading) return; // Prevent multiple loads
        isSceneLoading = true;      // Mark scene as loading

        // Optional: small delay to avoid accidental double click
        StartCoroutine(LoadSceneAsync(sceneName));
    }

    // Coroutine to load scene
    private System.Collections.IEnumerator LoadSceneAsync(string scene)
    {
        yield return new WaitForSeconds(0.1f); // tiny delay
        SceneManager.LoadScene(scene);         // Load the selected scene
    }

    // Optional: Call this from a Back button that should always return to HomeScene
    public void LoadHomeScene()
    {
        if (isSceneLoading) return;
        isSceneLoading = true;
        SceneManager.LoadScene("Home"); // Make sure scene name matches exactly
    }
}
