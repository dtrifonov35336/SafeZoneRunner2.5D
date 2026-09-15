using UnityEngine;

public class TouchControls : MonoBehaviour
{
    public static TouchControls Instance { get; private set; }

    public float MoveInput { get; private set; }
    public bool JumpRequested { get; private set; }

    private bool leftPressed = false;
    private bool rightPressed = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void SetLeft(bool pressed)
    {
        leftPressed = pressed;
        Recalculate();
    }

    public void SetRight(bool pressed)
    {
        rightPressed = pressed;
        Recalculate();
    }

    public void TriggerJump()
    {
        JumpRequested = true;
    }

    public bool ConsumeJump()
    {
        if (!JumpRequested) return false;
        JumpRequested = false;
        return true;
    }

    private void Recalculate()
    {
        if (leftPressed && !rightPressed) MoveInput = -1f;
        else if (rightPressed && !leftPressed) MoveInput = 1f;
        else MoveInput = 0f;
    }
}