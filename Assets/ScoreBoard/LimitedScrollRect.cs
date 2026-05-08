using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class LimitedScrollRect : MonoBehaviour
{
    public float minY = 0f;     // Minimum vertical position (lower limit)
    public float maxY = 500f;   // Maximum vertical position (upper limit)

    private ScrollRect scrollRect;
    private RectTransform content;

    void Start()
    {
        scrollRect = GetComponent<ScrollRect>();
        content = scrollRect.content;
    }

    void LateUpdate()
    {
        if (content == null) return;

        Vector2 anchoredPos = content.anchoredPosition;

        // Clamp vertical movement (y-axis only)
        anchoredPos.y = Mathf.Clamp(anchoredPos.y, minY, maxY);

        // Optionally, clamp horizontal movement too:
        // anchoredPos.x = Mathf.Clamp(anchoredPos.x, minX, maxX);

        content.anchoredPosition = anchoredPos;
    }
}
