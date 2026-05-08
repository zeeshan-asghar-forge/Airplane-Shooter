using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBarUI : MonoBehaviour
{
    [Header("UI References")]
    public Image fillImage;
    public TextMeshProUGUI healthText;

    [Header("Color Settings")]
    public Color fullHealthColor = Color.green;
    public Color midHealthColor = Color.yellow;
    public Color lowHealthColor = Color.red;

    [Header("Smooth Animation")]
    public float smoothSpeed = 8f; // fill speed

    private float targetFill = 1f;
    private float currentFill = 1f;

    void Update()
    {
        // Smoothly animate the bar fill (optimized)
        if (Mathf.Abs(currentFill - targetFill) > 0.001f)
        {
            currentFill = Mathf.Lerp(currentFill, targetFill, Time.deltaTime * smoothSpeed);
            fillImage.fillAmount = currentFill;

            // Update color smoothly based on fill
            fillImage.color = GetHealthColor(currentFill);
        }
    }

    public void SetHealth(int current, int max)
    {
        if (max <= 0) return;

        targetFill = Mathf.Clamp01((float)current / max);

        if (healthText != null)
            healthText.text = current + " / " + max;
    }

    private Color GetHealthColor(float fill)
    {
        // Green → Yellow → Red gradient
        if (fill > 0.5f)
            return Color.Lerp(midHealthColor, fullHealthColor, (fill - 0.5f) * 2f);
        else
            return Color.Lerp(lowHealthColor, midHealthColor, fill * 2f);
    }
}
