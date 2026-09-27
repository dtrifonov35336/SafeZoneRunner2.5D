using UnityEngine;
using UnityEngine.UI;

public class RunProgressUI3D : MonoBehaviour
{
    [Header("Run Manager")]
    public RunManager runManager;

    [Header("UI")]
    public GameObject root;
    public Image fill;

    [Header("Предпросмотр")]
    [Range(0f, 1f)]
    public float previewProgress = 0.5f;

    [Header("Флаг")]
    public float flagSize = 48f;

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

    private RectTransform backgroundRect;
    private RectTransform trackRect;
    private RectTransform fillRect;
    private RectTransform flagRect;

    private Image backgroundImage;
    private Image trackImage;
    private Image flagImage;

    private void Awake()
    {
        if (runManager == null)
        {
            runManager =
                FindFirstObjectByType<RunManager>();
        }

        if (root == null)
        {
            root = gameObject;
        }

        ResolveReferences();
        SetupLayout();
        ApplyStyle();
    }

    private void Start()
    {
        ResolveReferences();
        SetupLayout();
        ApplyStyle();
        UpdateVisual();
    }

    private void Update()
    {
        UpdateVisual();
    }

    private void ResolveReferences()
    {
        if (root == null)
        {
            root = gameObject;
        }

        Transform background =
            root.transform.Find(
                "Background"
            );

        if (background != null)
        {
            backgroundRect =
                background.GetComponent<
                    RectTransform
                >();

            backgroundImage =
                background.GetComponent<
                    Image
                >();
        }

        if (backgroundRect != null)
        {
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

            // Флаг находится внутри Background.
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
    }

    private void SetupLayout()
    {
        if (backgroundRect == null)
        {
            return;
        }

        backgroundRect.anchorMin =
            new Vector2(
                0f,
                1f
            );

        backgroundRect.anchorMax =
            new Vector2(
                1f,
                1f
            );

        backgroundRect.pivot =
            new Vector2(
                0.5f,
                1f
            );

        backgroundRect.anchoredPosition =
            new Vector2(
                0f,
                -2f
            );

        backgroundRect.sizeDelta =
            new Vector2(
                -24f,
                34f
            );

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

            trackRect.SetAsFirstSibling();
        }

        if (fillRect != null &&
            trackRect != null)
        {
            if (fillRect.parent != trackRect)
            {
                fillRect.SetParent(
                    trackRect,
                    false
                );
            }

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

        if (flagRect != null)
        {
            if (flagRect.parent != backgroundRect)
            {
                flagRect.SetParent(
                    backgroundRect,
                    false
                );
            }

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
    }

    private void ApplyStyle()
    {
        if (trackImage != null)
        {
            trackImage.color =
                progressBackgroundColor;

            if (trackImage.sprite == null &&
                backgroundImage != null)
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

    private void UpdateVisual()
    {
        if (root == null)
        {
            root = gameObject;
        }

        ResolveReferences();

        bool inPlayMode =
            Application.isPlaying;

        float progress;

        if (inPlayMode &&
            runManager != null)
        {
            bool visible =
                !runManager.infiniteRun;

            if (root.activeSelf != visible)
            {
                root.SetActive(
                    visible
                );
            }

            if (!visible)
            {
                return;
            }

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
        if (fillRect == null ||
            trackRect == null)
        {
            return;
        }

        float width =
            trackRect.rect.width;

        if (width <= 0.01f)
        {
            return;
        }

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
        if (backgroundRect == null ||
            flagRect == null)
        {
            return;
        }

        float backgroundWidth =
            backgroundRect.rect.width;

        if (backgroundWidth <= 0.01f)
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