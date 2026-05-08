using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverMenu : MonoBehaviour
{
    public void RestartGame()
    {
        // Fade music back in when restarting
        BackgroundMusicController.FadeInMusic();

        // Reload the currently active scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LoadMainMenu()
    {
        // Music continues as-is
        SceneManager.LoadScene("Menu");
    }
}
