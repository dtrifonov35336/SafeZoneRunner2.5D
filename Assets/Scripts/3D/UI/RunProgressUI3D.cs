using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RunProgressUI3D : MonoBehaviour
{
    [Header("Run Manager")]
    public RunManager runManager;

    [Header("UI")]
    public GameObject root;
    public Image fill;
    public TextMeshProUGUI label;

    [Header("Настройки")]
    [Range(0f, 1f)]
    public float startFill = 0f;

    [Header("Флаг прогресса")]
    public float flagSize = 48f;

    [Tooltip(
        "Отступ заполненной области Fill от краёв Background."
    )]
    public float fillInset = 3f;

    [Header("Цвет Fill")]
    public Color fillColor =
        new Color32(
            198,
            166,
            91,
            255
        );

    [Header("Цвет текста")]
    public Color labelColor =
        new Color32(
            224,
            191,
            120,
            255
        );

    [Header("Обводка текста")]
    public Color labelOutlineColor =
        new Color32(
            35,
            40,
            39,
            210
        );

    private RectTransform backgroundRect;
    private RectTransform flagRect;
    private Image flagImage;

    private void Awake()
    {
        if (runManager == null)
        {
            runManager =
                FindFirstObjectByType<RunManager>();
        }

        ResolveReferences();
        ApplyStyle();
        SetupFlagParent();
    }

    private void Start()
    {
        ResolveReferences();
        ApplyStyle();
        SetupFlagParent();

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

        if (backgroundRect == null)
        {
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
            }
        }

        if (flagRect == null)
        {
            Transform flag =
                root.transform.Find(
                    "Flag"
                );

            if (flag != null)
            {
                flagRect =
                    flag.GetComponent<
                        RectTransform
                    >();

                if (flagRect != null)
                {
                    flagImage =
                        flagRect.GetComponent<
                            Image
                        >();
                }
            }
        }
    }

    private void ApplyStyle()
    {
        if (fill != null)
        {
            fill.color =
                fillColor;
        }

        if (label != null)
        {
            label.color =
                labelColor;

            label.outlineColor =
                labelOutlineColor;

            label.outlineWidth =
                0.18f;
        }

        if (flagRect != null)
        {
            flagRect.sizeDelta =
                new Vector2(
                    flagSize,
                    flagSize
                );
        }

        if (flagImage != null)
        {
            flagImage.preserveAspect =
                true;
        }
    }

    private void SetupFlagParent()
    {
        if (backgroundRect == null ||
            flagRect == null)
        {
            return;
        }

        /*
         * Флаг должен находиться внутри Background,
         * чтобы его координаты напрямую соответствовали
         * длине самой полосы.
         */
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
    }

    private void UpdateVisual()
    {
        if (runManager == null)
        {
            return;
        }

        bool visible =
            !runManager.infiniteRun;

        if (root != null &&
            root.activeSelf != visible)
        {
            root.SetActive(
                visible
            );
        }

        if (!visible)
        {
            return;
        }

        ResolveReferences();
        SetupFlagParent();
        ApplyStyle();

        float progress =
            Mathf.Clamp01(
                runManager.GetRunProgress()
            );

        if (fill != null)
        {
            fill.fillAmount =
                progress;
        }

        UpdateFlagPosition(
            progress
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

        /*
         * Fill находится внутри Background
         * с отступами 3 px с каждой стороны.
         */
        float left =
            fillInset;

        float right =
            Mathf.Max(
                left,
                backgroundWidth -
                fillInset
            );

        /*
         * Учитываем размер самой иконки,
         * чтобы флаг не вылезал за границы полосы.
         */
        float halfFlag =
            flagSize * 0.5f;

        float minX =
            left +
            halfFlag;

        float maxX =
            Mathf.Max(
                minX,
                right -
                halfFlag
            );

        /*
         * Флаг движется строго вместе
         * с концом Fill.
         */
        float x =
            Mathf.Lerp(
                minX,
                maxX,
                progress
            );

        flagRect.anchoredPosition =
            new Vector2(
                x,
                0f
            );
    }
}