using UnityEngine;
using UnityEngine.InputSystem;

public class TouchControls : MonoBehaviour
{
    public static TouchControls Instance { get; private set; }

    [Header("Настройки свайпа")]
    [Tooltip("Минимальное расстояние свайпа в пикселях.")]
    public float swipeMinDistance = 80f;

    [Tooltip("Максимальное время, за которое должен произойти свайп.")]
    public float swipeMaxTime = 0.45f;

    [Header("Двойное нажатие")]
    [Tooltip("Максимальное время между двумя нажатиями.")]
    public float doubleTapTime = 0.28f;

    [Tooltip("Максимальное расстояние между двумя нажатиями.")]
    public float doubleTapMaxDistance = 100f;

    [Header("Альтернативная схема (двойной прыжок)")]
    [Tooltip(
        "Если включено: двойной прыжок выполняется двумя " +
        "последовательными свайпами вверх. По умолчанию выключено, " +
        "чтобы не менять управление в обычных забегах."
    )]
    public bool alternativeDoubleJumpSwipe = false;

    [Tooltip(
        "Максимальное время между двумя свайпами вверх " +
        "для альтернативного двойного прыжка."
    )]
    public float doubleSwipeWindow = 0.6f;

    private const string DoubleJumpControlKey =
    "Settings_DoubleJumpControl";

    public DoubleJumpControlMode ControlMode { get; private set; }
        = DoubleJumpControlMode.DoubleTap;

    public static DoubleJumpControlMode SavedControlMode =>
        (DoubleJumpControlMode)Mathf.Clamp(
            PlayerPrefs.GetInt(
                DoubleJumpControlKey,
                (int)DoubleJumpControlMode.DoubleTap),
            0,
            2);

    public bool LeftRequested { get; private set; }
    public bool RightRequested { get; private set; }
    public bool JumpRequested { get; private set; }
    public bool DoubleJumpRequested { get; private set; }
    public bool SlideRequested { get; private set; }

    private bool trackingTouch;
    private Vector2 touchStartPosition;
    private float touchStartTime;

    private bool waitingForSecondTap;
    private float firstTapTime;
    private Vector2 firstTapPosition;

    private float lastUpSwipeTime = -100f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        LoadControlMode();
    }

    private void Update()
    {
        ProcessTouch();
        ProcessMouseForEditor();
        CheckDoubleTapTimeout();
    }

    private void ProcessTouch()
    {
        if (Touchscreen.current == null)
            return;

        var touch = Touchscreen.current.primaryTouch;

        if (touch.press.wasPressedThisFrame)
        {
            BeginTouch(touch.position.ReadValue());
        }

        if (trackingTouch && touch.press.isPressed)
        {
            CheckSwipeWhileMoving(touch.position.ReadValue());
        }

        if (trackingTouch && touch.press.wasReleasedThisFrame)
        {
            EndTouch(touch.position.ReadValue());
        }
    }

    private void ProcessMouseForEditor()
    {
        if (Mouse.current == null)
            return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            BeginTouch(Mouse.current.position.ReadValue());
        }

        if (trackingTouch && Mouse.current.leftButton.isPressed)
        {
            CheckSwipeWhileMoving(Mouse.current.position.ReadValue());
        }

        if (trackingTouch && Mouse.current.leftButton.wasReleasedThisFrame)
        {
            EndTouch(Mouse.current.position.ReadValue());
        }
    }

    private void BeginTouch(Vector2 position)
    {
        // Если уже отслеживаем другое касание — игнорируем.
        if (trackingTouch)
            return;

        trackingTouch = true;
        touchStartPosition = position;
        touchStartTime = Time.unscaledTime;
    }

    private void CheckSwipeWhileMoving(Vector2 currentPosition)
    {
        Vector2 delta = currentPosition - touchStartPosition;

        if (delta.magnitude < swipeMinDistance)
            return;

        float elapsed = Time.unscaledTime - touchStartTime;

        if (elapsed > swipeMaxTime)
            return;

        // Свайп состоялся.
        trackingTouch = false;

        float absX = Mathf.Abs(delta.x);
        float absY = Mathf.Abs(delta.y);

        if (absX > absY)
        {
            // Горизонтальный свайп.
            if (delta.x < 0f)
                LeftRequested = true;
            else
                RightRequested = true;
        }
        else
        {
            // Вертикальный свайп.
            if (delta.y > 0f)
            {
                if (ControlMode == DoubleJumpControlMode.DoubleSwipeUp &&
                    Time.unscaledTime - lastUpSwipeTime <= doubleSwipeWindow)
                {
                    lastUpSwipeTime = -100f;
                    DoubleJumpRequested = true;
                }
                else
                {
                    lastUpSwipeTime = Time.unscaledTime;
                    JumpRequested = true;
                }
            }
            else
            {
                SlideRequested = true;
            }
        }
    }

    private void EndTouch(Vector2 releasePosition)
    {
        if (!trackingTouch)
            return;

        trackingTouch = false;

        Vector2 delta = releasePosition - touchStartPosition;

        // Если это было не перемещение, а короткое нажатие —
        // проверяем двойное нажатие.
        if (delta.magnitude < swipeMinDistance)
        {
            RegisterTap(releasePosition);
        }
    }

    private void RegisterTap(Vector2 position)
    {
        float now = Time.unscaledTime;

        if (waitingForSecondTap)
        {
            float elapsed = now - firstTapTime;
            float distance = Vector2.Distance(
                position,
                firstTapPosition);

            if (elapsed <= doubleTapTime &&
                distance <= doubleTapMaxDistance)
            {
                waitingForSecondTap = false;

                if (ControlMode != DoubleJumpControlMode.DoubleSwipeUp)
                {
                    DoubleJumpRequested = true;
                }

                return;
            }
        }

        waitingForSecondTap = true;
        firstTapTime = now;
        firstTapPosition = position;
    }

    private void CheckDoubleTapTimeout()
    {
        if (!waitingForSecondTap)
            return;

        if (Time.unscaledTime - firstTapTime > doubleTapTime)
        {
            waitingForSecondTap = false;
        }
    }

    public bool ConsumeLeft()
    {
        if (!LeftRequested)
            return false;

        LeftRequested = false;
        return true;
    }

    public bool ConsumeRight()
    {
        if (!RightRequested)
            return false;

        RightRequested = false;
        return true;
    }

    public bool ConsumeJump()
    {
        if (!JumpRequested)
            return false;

        JumpRequested = false;
        return true;
    }

    public bool ConsumeDoubleJump()
    {
        if (!DoubleJumpRequested)
            return false;

        DoubleJumpRequested = false;
        return true;
    }

    public bool ConsumeSlide()
    {
        if (!SlideRequested)
            return false;

        SlideRequested = false;
        return true;
    }

    public void SetControlMode(DoubleJumpControlMode mode)
    {
        ControlMode = mode;
        alternativeDoubleJumpSwipe =
            mode == DoubleJumpControlMode.DoubleSwipeUp;

        PlayerPrefs.SetInt(
            DoubleJumpControlKey,
            (int)mode);

        PlayerPrefs.Save();

        waitingForSecondTap = false;
        lastUpSwipeTime = -100f;

        JumpRequested = false;
        DoubleJumpRequested = false;
    }

    private void LoadControlMode()
    {
        ControlMode = SavedControlMode;

        alternativeDoubleJumpSwipe =
            ControlMode == DoubleJumpControlMode.DoubleSwipeUp;
    }
}