using UnityEngine;

[ExecuteAlways]
public class MainMenuLogoResponsive3D : MonoBehaviour
{
    [Header("Ссылки")]
    [SerializeField]
    private RectTransform safeArea;

    [SerializeField]
    private RectTransform profileBox;

    [SerializeField]
    private RectTransform coinsBox;

    [SerializeField]
    private RectTransform diamondsBox;

    [Header("Game Logo")]
    [SerializeField]
    private RectTransform logo;

    [Header("Широкий экран")]
    [SerializeField]
    [Range(0.5f, 0.8f)]
    private float wideAspectThreshold = 0.60f;

    [SerializeField]
    private float wideHorizontalPadding = 20f;

    [SerializeField]
    private float wideVerticalOffset = 0f;

    [Header("Узкий экран")]
    [SerializeField]
    private float narrowGapBelowTopUI = 20f;

    [SerializeField]
    private float narrowScale = 1f;

    [Header("Автопоиск")]
    [SerializeField]
    private bool autoFindReferences = true;

    private Vector2 originalSize;
    private bool originalSizeCaptured;

    private void Awake()
    {
        CaptureOriginalSize();

        if (autoFindReferences)
        {
            FindReferences();
        }

        Refresh();
    }

    private void Start()
    {
        CaptureOriginalSize();

        if (autoFindReferences)
        {
            FindReferences();
        }

        Refresh();
    }

    private void Update()
    {
        Refresh();
    }

#if UNITY_EDITOR

    private void OnValidate()
    {
        CaptureOriginalSize();

        if (autoFindReferences)
        {
            FindReferences();
        }

        Refresh();
    }

#endif

    // =========================================================
    // REFRESH
    // =========================================================

    private void Refresh()
    {
        if (logo == null)
            return;

        FindReferences();

        if (safeArea == null)
            return;

        if (!originalSizeCaptured)
        {
            CaptureOriginalSize();
        }

        float aspect =
            GetAspect();

        bool wide =
            aspect >=
            wideAspectThreshold;

        if (wide)
        {
            ApplyWideLayout();
        }
        else
        {
            ApplyNarrowLayout();
        }
    }

    // =========================================================
    // WIDE
    // =========================================================

    private void ApplyWideLayout()
    {
        if (
            profileBox == null ||
            coinsBox == null ||
            diamondsBox == null
        )
        {
            ApplyNarrowLayout();
            return;
        }

        RectBounds profile =
            GetBounds(profileBox);

        RectBounds coins =
            GetBounds(coinsBox);

        RectBounds diamonds =
            GetBounds(diamondsBox);

        float currencyLeft =
            Mathf.Min(
                coins.left,
                diamonds.left
            );

        float gap =
            currencyLeft -
            profile.right;

        float requiredWidth =
            originalSize.x +
            wideHorizontalPadding *
            2f;

        // Если места реально не хватает,
        // считаем экран узким.
        if (gap < requiredWidth)
        {
            ApplyNarrowLayout();
            return;
        }

        float targetX =
            (
                profile.right +
                currencyLeft
            ) *
            0.5f;

        float targetY =
            (
                profile.centerY +
                (
                    coins.centerY +
                    diamonds.centerY
                ) *
                0.5f
            ) *
            0.5f;

        SetCenterAnchoring();

        logo.anchoredPosition =
            new Vector2(
                targetX,
                targetY +
                wideVerticalOffset
            );

        logo.sizeDelta =
            originalSize;

        logo.localScale =
            Vector3.one;
    }

    // =========================================================
    // NARROW
    // =========================================================

    private void ApplyNarrowLayout()
    {
        SetCenterAnchoring();

        float safeTop =
            safeArea.rect.height *
            0.5f;

        float topUiBottom =
            safeTop;

        if (profileBox != null)
        {
            RectBounds profile =
                GetBounds(
                    profileBox
                );

            topUiBottom =
                Mathf.Min(
                    topUiBottom,
                    profile.bottom
                );
        }

        if (coinsBox != null)
        {
            RectBounds coins =
                GetBounds(
                    coinsBox
                );

            topUiBottom =
                Mathf.Min(
                    topUiBottom,
                    coins.bottom
                );
        }

        if (diamondsBox != null)
        {
            RectBounds diamonds =
                GetBounds(
                    diamondsBox
                );

            topUiBottom =
                Mathf.Min(
                    topUiBottom,
                    diamonds.bottom
                );
        }

        float logoHeight =
            originalSize.y;

        float centerY =
            topUiBottom -
            narrowGapBelowTopUI -
            logoHeight *
            0.5f;

        logo.anchoredPosition =
            new Vector2(
                0f,
                centerY
            );

        logo.sizeDelta =
            originalSize;

        logo.localScale =
            Vector3.one *
            narrowScale;
    }

    // =========================================================
    // ANCHOR
    // =========================================================

    private void SetCenterAnchoring()
    {
        if (logo.parent != safeArea)
        {
            logo.SetParent(
                safeArea,
                false
            );
        }

        logo.anchorMin =
            new Vector2(
                0.5f,
                0.5f
            );

        logo.anchorMax =
            new Vector2(
                0.5f,
                0.5f
            );

        logo.pivot =
            new Vector2(
                0.5f,
                0.5f
            );
    }

    // =========================================================
    // REFERENCES
    // =========================================================

    private void FindReferences()
    {
        if (!autoFindReferences)
            return;

        Canvas canvas =
            FindFirstObjectByType<Canvas>();

        if (canvas == null)
            return;

        if (safeArea == null)
        {
            Transform found =
                canvas.transform.Find(
                    "SafeArea"
                );

            if (found != null)
            {
                safeArea =
                    found.GetComponent<
                        RectTransform
                    >();
            }
        }

        if (
            safeArea == null
        )
        {
            return;
        }

        if (profileBox == null)
        {
            Transform found =
                FindDeepChild(
                    safeArea,
                    "ProfileBox"
                );

            if (found != null)
            {
                profileBox =
                    found.GetComponent<
                        RectTransform
                    >();
            }
        }

        if (coinsBox == null)
        {
            Transform found =
                FindDeepChild(
                    safeArea,
                    "CoinsBox"
                );

            if (found != null)
            {
                coinsBox =
                    found.GetComponent<
                        RectTransform
                    >();
            }
        }

        if (diamondsBox == null)
        {
            Transform found =
                FindDeepChild(
                    safeArea,
                    "DiamondsBox"
                );

            if (found != null)
            {
                diamondsBox =
                    found.GetComponent<
                        RectTransform
                    >();
            }
        }

        if (logo == null)
        {
            Transform found =
                FindDeepChild(
                    canvas.transform,
                    "GameLogo"
                );

            if (found != null)
            {
                logo =
                    found.GetComponent<
                        RectTransform
                    >();
            }
        }
    }

    private Transform FindDeepChild(
        Transform parent,
        string childName
    )
    {
        foreach (
            Transform child
            in parent
        )
        {
            if (
                child.name ==
                childName
            )
            {
                return child;
            }

            Transform result =
                FindDeepChild(
                    child,
                    childName
                );

            if (result != null)
                return result;
        }

        return null;
    }

    // =========================================================
    // ORIGINAL SIZE
    // =========================================================

    private void CaptureOriginalSize()
    {
        if (
            logo == null ||
            originalSizeCaptured
        )
        {
            return;
        }

        originalSize =
            logo.sizeDelta;

        originalSizeCaptured =
            true;
    }

    // =========================================================
    // ASPECT
    // =========================================================

    private float GetAspect()
    {
        if (
            Screen.width > 10 &&
            Screen.height > 10
        )
        {
            return
                (float)Screen.width /
                Screen.height;
        }

#if UNITY_EDITOR

        UnityEngine.Vector2 size =
            UnityEditor.Handles.GetMainGameViewSize();

        if (
            size.x > 10f &&
            size.y > 10f
        )
        {
            return
                size.x /
                size.y;
        }

#endif

        return
            1080f /
            1920f;
    }

    // =========================================================
    // BOUNDS
    // =========================================================

    private RectBounds GetBounds(
        RectTransform rect
    )
    {
        Vector3[] corners =
            new Vector3[4];

        rect.GetWorldCorners(
            corners
        );

        Vector3 bottomLeft =
            safeArea.InverseTransformPoint(
                corners[0]
            );

        Vector3 topRight =
            safeArea.InverseTransformPoint(
                corners[2]
            );

        return new RectBounds(
            bottomLeft.x,
            topRight.x,
            bottomLeft.y,
            topRight.y
        );
    }

    private struct RectBounds
    {
        public float left;
        public float right;
        public float bottom;
        public float top;

        public float centerY =>
            (bottom + top) *
            0.5f;

        public RectBounds(
            float left,
            float right,
            float bottom,
            float top
        )
        {
            this.left =
                left;

            this.right =
                right;

            this.bottom =
                bottom;

            this.top =
                top;
        }
    }
}