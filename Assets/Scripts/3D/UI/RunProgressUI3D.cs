using UnityEngine;
using UnityEngine.UI;

public class RunProgressUI3D : MonoBehaviour
{
    [Header("Run Manager")]
    public RunManager runManager;

    [Header("UI")]
    public GameObject root;
    public Image fill;

    [Header("Layout")]
    [Min(180f)]
    public float progressWidth = 330f;

    [Min(30f)]
    public float progressHeight = 46f;

    [Min(0f)]
    public float bottomMargin = 285f;

    [Header("Предпросмотр")]
    [Range(0f, 1f)]
    public float previewProgress = 0.5f;

    [Header("Флаг")]
    public float flagSize = 34f;

    [Header("Отступ")]
    public float fillInset = 4f;

    [Header("Цвет фона")]
    public Color progressBackgroundColor =
        new Color32(
            48,
            54,
            53,
            245
        );

    [Header("Цвет Fill")]
    public Color fillColor =
        new Color32(
            222,
            190,
            102,
            255
        );

    private RectTransform rootRect;
    private RectTransform backgroundRect;
    private RectTransform trackRect;
    private RectTransform fillRect;
    private RectTransform flagRect;

    private Image backgroundImage;
    private Image trackImage;
    private Image flagImage;

    private Canvas canvas;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        if (root == null)
        {
            root =
                gameObject;
        }

        if (runManager == null)
        {
            runManager =
                FindFirstObjectByType<RunManager>();
        }

        canvas =
            GetComponentInParent<Canvas>(
                true
            );

        if (canvas == null)
        {
            canvas =
                FindFirstObjectByType<Canvas>();
        }

        EnsureCanvasParent();

        canvasGroup =
            root.GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup =
                root.AddComponent<CanvasGroup>();
        }

        ResolveReferences();
        SetupLayout();
        ApplyStyle();
    }

    private void Start()
    {
        if (runManager == null)
        {
            runManager =
                FindFirstObjectByType<RunManager>();
        }

        if (canvas == null)
        {
            canvas =
                GetComponentInParent<Canvas>(
                    true
                );

            if (canvas == null)
            {
                canvas =
                    FindFirstObjectByType<Canvas>();
            }
        }

        EnsureCanvasParent();

        ResolveReferences();
        SetupLayout();
        ApplyStyle();
        UpdateVisual();
    }

    private void Update()
    {
        UpdateVisual();
    }

    // =========================================================
    // CANVAS
    // =========================================================

    private void EnsureCanvasParent()
    {
        if (canvas == null ||
            root == null)
        {
            return;
        }

        if (
            root.transform.parent !=
            canvas.transform
        )
        {
            root.transform.SetParent(
                canvas.transform,
                false
            );
        }
    }

    // =========================================================
    // REFERENCES
    // =========================================================

    private void ResolveReferences()
    {
        if (root == null)
        {
            root =
                gameObject;
        }

        rootRect =
            root.GetComponent<
                RectTransform
            >();

        Transform background =
            root.transform.Find(
                "Background"
            );

        if (background == null)
            return;

        backgroundRect =
            background.GetComponent<
                RectTransform
            >();

        backgroundImage =
            background.GetComponent<
                Image
            >();

        if (backgroundRect == null)
            return;

        Transform track =
            backgroundRect.Find(
                "ProgressBackground"
            );

        if (track != null)
        {
            trackRect =
                track.GetComponent<
                    RectTransform
                >();

            trackImage =
                track.GetComponent<
                    Image
                >();

            Transform trackFill =
                track.Find(
                    "Fill"
                );

            if (trackFill != null)
            {
                fillRect =
                    trackFill.GetComponent<
                        RectTransform
                    >();

                Image foundFill =
                    trackFill.GetComponent<
                        Image
                    >();

                if (foundFill != null)
                {
                    fill =
                        foundFill;
                }
            }
        }

        Transform flag =
            backgroundRect.Find(
                "Flag"
            );

        if (flag != null)
        {
            flagRect =
                flag.GetComponent<
                    RectTransform
                >();

            flagImage =
                flag.GetComponent<
                    Image
                >();
        }
    }

    // =========================================================
    // LAYOUT
    // =========================================================

    private void SetupLayout()
    {
        if (rootRect == null)
            return;

        // Центр экрана, над зомби.
        rootRect.anchorMin =
            new Vector2(
                0.5f,
                0f
            );

        rootRect.anchorMax =
            new Vector2(
                0.5f,
                0f
            );

        rootRect.pivot =
            new Vector2(
                0.5f,
                0f
            );

        rootRect.anchoredPosition =
            new Vector2(
                0f,
                bottomMargin
            );

        rootRect.sizeDelta =
            new Vector2(
                progressWidth,
                progressHeight
            );

        rootRect.localScale =
            Vector3.one;

        // =====================================================
        // BACKGROUND
        // =====================================================

        if (backgroundRect != null)
        {
            backgroundRect.anchorMin =
                Vector2.zero;

            backgroundRect.anchorMax =
                Vector2.one;

            backgroundRect.pivot =
                new Vector2(
                    0.5f,
                    0.5f
                );

            backgroundRect.offsetMin =
                Vector2.zero;

            backgroundRect.offsetMax =
                Vector2.zero;

            backgroundRect.anchoredPosition =
                Vector2.zero;

            backgroundRect.localScale =
                Vector3.one;
        }

        // =====================================================
        // TRACK
        // =====================================================

        if (trackRect != null)
        {
            trackRect.anchorMin =
                Vector2.zero;

            trackRect.anchorMax =
                Vector2.one;

            trackRect.offsetMin =
                new Vector2(
                    fillInset,
                    fillInset
                );

            trackRect.offsetMax =
                new Vector2(
                    -fillInset,
                    -fillInset
                );

            trackRect.anchoredPosition =
                Vector2.zero;

            trackRect.localScale =
                Vector3.one;

            trackRect.SetAsFirstSibling();
        }

        // =====================================================
        // FILL
        // =====================================================

        if (
            fillRect != null &&
            trackRect != null
        )
        {
            fillRect.anchorMin =
                new Vector2(
                    0f,
                    0f
                );

            fillRect.anchorMax =
                new Vector2(
                    0f,
                    1f
                );

            fillRect.pivot =
                new Vector2(
                    0f,
                    0.5f
                );

            fillRect.anchoredPosition =
                Vector2.zero;

            fillRect.localScale =
                Vector3.one;

            fillRect.SetAsLastSibling();
        }

        // =====================================================
        // FLAG
        // =====================================================

        if (flagRect != null)
        {
            flagRect.anchorMin =
                new Vector2(
                    0f,
                    0.5f
                );

            flagRect.anchorMax =
                new Vector2(
                    0f,
                    0.5f
                );

            flagRect.pivot =
                new Vector2(
                    0.5f,
                    0.5f
                );

            flagRect.sizeDelta =
                new Vector2(
                    flagSize,
                    flagSize
                );

            flagRect.localScale =
                Vector3.one;

            flagRect.SetAsLastSibling();
        }

        root.transform.SetAsLastSibling();
    }

    // =========================================================
    // STYLE
    // =========================================================

    private void ApplyStyle()
    {
        if (backgroundImage != null)
        {
            backgroundImage.color =
                new Color32(
                    23,
                    27,
                    28,
                    235
                );

            backgroundImage.raycastTarget =
                false;
        }

        if (trackImage != null)
        {
            trackImage.color =
                progressBackgroundColor;

            if (
                trackImage.sprite == null &&
                backgroundImage != null
            )
            {
                trackImage.sprite =
                    backgroundImage.sprite;

                trackImage.type =
                    backgroundImage.type;
            }

            trackImage.raycastTarget =
                false;
        }

        if (fill != null)
        {
            fill.color =
                fillColor;

            fill.raycastTarget =
                false;
        }

        if (flagImage != null)
        {
            flagImage.preserveAspect =
                true;

            flagImage.raycastTarget =
                false;
        }
    }

    // =========================================================
    // VISUAL
    // =========================================================

    private void UpdateVisual()
    {
        if (root == null)
        {
            root =
                gameObject;
        }

        if (canvasGroup == null)
        {
            canvasGroup =
                root.GetComponent<
                    CanvasGroup
                >();

            if (canvasGroup == null)
            {
                canvasGroup =
                    root.AddComponent<
                        CanvasGroup
                    >();
            }
        }

        if (runManager == null)
        {
            runManager =
                FindFirstObjectByType<RunManager>();
        }

        ResolveReferences();

        bool gameplayVisible =
            true;

        if (
            Application.isPlaying &&
            runManager != null
        )
        {
            // Бесконечный режим — шкала убежища не нужна.
            if (runManager.infiniteRun)
            {
                gameplayVisible =
                    false;
            }

            // =================================================
            // СМЕРТЬ ИЛИ ПОБЕДА
            // =================================================

            ChaseManager chase =
                ChaseManager.Instance;

            if (
                chase != null &&
                chase.IsGameOver()
            )
            {
                gameplayVisible =
                    false;
            }
        }

        canvasGroup.alpha =
            gameplayVisible
                ? 1f
                : 0f;

        canvasGroup.interactable =
            false;

        canvasGroup.blocksRaycasts =
            false;

        float progress;

        if (
            Application.isPlaying &&
            runManager != null
        )
        {
            progress =
                Mathf.Clamp01(
                    runManager.GetRunProgress()
                );
        }
        else
        {
            progress =
                Mathf.Clamp01(
                    previewProgress
                );
        }

        UpdateFill(
            progress
        );

        UpdateFlagPosition(
            progress
        );
    }

    private void UpdateFill(
        float progress
    )
    {
        if (
            fillRect == null ||
            trackRect == null
        )
        {
            return;
        }

        float width =
            trackRect.rect.width;

        if (width <= 0.01f)
            return;

        fillRect.sizeDelta =
            new Vector2(
                width * progress,
                0f
            );
    }

    private void UpdateFlagPosition(
        float progress
    )
    {
        if (
            backgroundRect == null ||
            flagRect == null
        )
        {
            return;
        }

        float backgroundWidth =
            backgroundRect.rect.width;

        if (
            backgroundWidth <=
            0.01f
        )
        {
            return;
        }

        float trackLeft =
            fillInset;

        float trackWidth =
            backgroundWidth -
            fillInset * 2f;

        trackWidth =
            Mathf.Max(
                0f,
                trackWidth
            );

        float halfFlag =
            flagSize * 0.5f;

        float minX =
            trackLeft +
            halfFlag;

        float maxX =
            trackLeft +
            trackWidth -
            halfFlag;

        float x =
            Mathf.Lerp(
                minX,
                Mathf.Max(
                    minX,
                    maxX
                ),
                progress
            );

        flagRect.anchoredPosition =
            new Vector2(
                x,
                0f
            );
    }
}