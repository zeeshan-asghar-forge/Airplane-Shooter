using TMPro;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("Damage & Heal %")]
    [Range(1, 20)] public int coneDamagePercent = 5;
    [Range(1, 30)] public int SpikesDamagePercent = 20;
    [Range(1, 50)] public int rampHealPercent = 10;

    public HealthBarUI healthBar;
    public GameObject gameOverPanel;
    public BallController ballController;

    private int Score;
    public TMP_Text HighscoreText;
    public GameObject NewHighScore;

    void Start()
    {
        currentHealth = maxHealth;
        Time.timeScale = 1f;

        if (healthBar != null)
            healthBar.SetHealth(currentHealth, maxHealth);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    public void TakeDamagePercent(int percent)
    {
        int amount = Mathf.RoundToInt(maxHealth * percent / 100f);
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (healthBar != null)
            healthBar.SetHealth(currentHealth, maxHealth);

        if (ballController == null)
            ballController = FindAnyObjectByType<BallController>();

        if (currentHealth <= 0)
            GameOver();
    }

    public void HealPercent(int percent)
    {
        int amount = Mathf.RoundToInt(maxHealth * percent / 100f);
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (healthBar != null)
            healthBar.SetHealth(currentHealth, maxHealth);
    }

    public void GameOver()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        // Fade out music when player dies
        BackgroundMusicController.FadeOutMusic();

        ballController.isDead = true;
        Score = ballController.score;
        int HighestScore = PlayerPrefs.GetInt("Highscore", 0);

        if (Score > HighestScore)
        {
            NewHighScore.SetActive(true);
            HighestScore = Score;
            PlayerPrefs.SetInt("Highscore", HighestScore);
        }

        HighscoreText.text = HighestScore.ToString("N0");
    }

    public void Revive()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        ballController.isDead = false;
        HealPercent(100);

        // Fade music back in when player revives
        BackgroundMusicController.FadeInMusic();
    }
}
