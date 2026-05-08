using UnityEngine;
using UnityEngine.EventSystems;

public class MobileInputManager : MonoBehaviour
{
    [Header("Assign Buttons Canvas")]
    public GameObject buttonsCanvas;

    [Header("Assign Buttons")]
    public GameObject leftButton;
    public GameObject rightButton;
    public GameObject jumpButton;

    [HideInInspector] public bool leftPressed;
    [HideInInspector] public bool rightPressed;
    [HideInInspector] public bool jumpPressed;

    private bool jumpConsumed = false; // ✅ used for IsJumpPressedOnce()

    [Header("Editor Testing")]
    public bool forceShowInEditor = true;

    private BallController ballController; // ✅ to detect when player dies

    void Awake()
    {
        // ✅ Always show canvas on all devices
        if (buttonsCanvas != null)
            buttonsCanvas.SetActive(true);
    }

    void Start()
    {
        // ✅ cache BallController reference
        ballController = FindAnyObjectByType<BallController>();

        if (buttonsCanvas != null && buttonsCanvas.activeSelf)
        {
            AddButtonEvents(leftButton, "left");
            AddButtonEvents(rightButton, "right");
            AddButtonEvents(jumpButton, "jump");
        }
    }

    void Update()
    {
        // ✅ Hide buttons when player dies (without allocations)
        if (ballController != null && ballController.isDead && buttonsCanvas.activeSelf)
        {
            buttonsCanvas.SetActive(false);
        }
    }

    void AddButtonEvents(GameObject buttonObj, string type)
    {
        if (buttonObj == null) return;

        EventTrigger trigger = buttonObj.GetComponent<EventTrigger>();
        if (trigger == null) trigger = buttonObj.AddComponent<EventTrigger>();

        trigger.triggers.Clear();

        EventTrigger.Entry pointerDown = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
        pointerDown.callback.AddListener((_) => SetButtonState(type, true));
        trigger.triggers.Add(pointerDown);

        EventTrigger.Entry pointerUp = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
        pointerUp.callback.AddListener((_) => SetButtonState(type, false));
        trigger.triggers.Add(pointerUp);
    }

    void SetButtonState(string type, bool state)
    {
        switch (type)
        {
            case "left": leftPressed = state; break;
            case "right": rightPressed = state; break;
            case "jump":
                jumpPressed = state;
                if (!state) jumpConsumed = false; // reset when released
                break;
        }
    }

    // ✅ Called once when jump button is tapped
    public bool IsJumpPressedOnce()
    {
        if (jumpPressed && !jumpConsumed)
        {
            jumpConsumed = true;
            return true;
        }
        return false;
    }

    public bool IsLeftPressed() => leftPressed;
    public bool IsRightPressed() => rightPressed;
    public bool IsJumpPressed() => jumpPressed;
}
