using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LevelManager : MonoBehaviour
{
    [Header("Timer UI")]
    [SerializeField] private Slider timeSlider;
    [SerializeField] private float totalTime = 60f;
    [SerializeField] private bool startAutomatically = true;

    [Header("Health Settings")]
    public int maxHealth = 100;
    public int asteroidDamage = 30;

    [Header("Health UI")]
    [SerializeField] private Image healthBarFill;
    [SerializeField] private Image ghostBarFill;
    [SerializeField] private Color fullHealthColor = new Color(0f, 0.9f, 1f); // Cyan
    [SerializeField] private Color midHealthColor = new Color(0f, 0.5f, 1f);  // Blue
    [SerializeField] private Color lowHealthColor = new Color(1f, 0f, 1f);    // Magenta

    private float _currentTime;
    private bool _isTimerActive;
    private int _currentHealth;
    private Coroutine _healthCoroutine;

    void Start()
    {
        if (timeSlider == null) Debug.LogError("LevelManager: Time Slider not assigned!");
        ResetTimer();
        if (startAutomatically) StartTimer();
    }

    void Update()
    {
        if (!_isTimerActive) return;
        if (_currentTime > 0)
        {
            _currentTime -= Time.deltaTime;
            timeSlider.value = _currentTime;
        }
        else
        {
            TimerReachedZero();
        }
    }

    // --- Timer Logic ---
    public void StartTimer() => _isTimerActive = true;
    public void PauseTimer() => _isTimerActive = false;
    public void ResetTimer()
    {
        _currentTime = totalTime;
        timeSlider.maxValue = totalTime;
        timeSlider.value = totalTime;
    }

    private void TimerReachedZero()
    {
        _currentTime = 0;
        timeSlider.value = 0;
        _isTimerActive = false;
    }

    // --- Health Logic ---
    public void SetupHealth()
    {
        _currentHealth = maxHealth;
        UpdateHealthUI(true);
    }

    public void UpdateHealth(int newHealth)
    {
        _currentHealth = newHealth;
        UpdateHealthUI(false);
    }

    private void UpdateHealthUI(bool instant)
    {
        if (!healthBarFill) return;

        float n = (float)_currentHealth / maxHealth;
        healthBarFill.fillAmount = n;

        if (n > 0.5f) healthBarFill.color = Color.Lerp(midHealthColor, fullHealthColor, (n - 0.5f) * 2f);
        else healthBarFill.color = Color.Lerp(lowHealthColor, midHealthColor, n * 2f);

        if (instant)
        {
            if (ghostBarFill) ghostBarFill.fillAmount = n;
        }
        else
        {
            if (_healthCoroutine != null) StopCoroutine(_healthCoroutine);
            _healthCoroutine = StartCoroutine(AnimateGhostBar(n));
        }
    }

    private IEnumerator AnimateGhostBar(float targetFill)
    {
        yield return new WaitForSeconds(0.5f);

        while (ghostBarFill && Mathf.Abs(ghostBarFill.fillAmount - targetFill) > 0.001f)
        {
            ghostBarFill.fillAmount = Mathf.Lerp(ghostBarFill.fillAmount, targetFill, Time.deltaTime * 5f);
            yield return null;
        }
        if (ghostBarFill) ghostBarFill.fillAmount = targetFill;
    }
}