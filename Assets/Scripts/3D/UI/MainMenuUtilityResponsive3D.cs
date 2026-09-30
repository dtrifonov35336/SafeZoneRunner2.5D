using UnityEngine;

[ExecuteAlways]
public class MainMenuUtilityResponsive3D : MonoBehaviour
{
    [Header("Safe Area")]
    [SerializeField]
    private RectTransform safeArea;

    [Header("Размер кнопок")]
    [SerializeField]
    private Vector2 buttonSize =
        new Vector2(
            140f,
            145f
        );

    [Header("Положение")]
    [SerializeField]
    private float leftOffset = 110f;

    [SerializeField]
    [Range(0.1f, 0.6f)]
    private float firstButtonTopRatio = 0.34f;

    [SerializeField]
    private float buttonStep = 150f;

    [Header("Минимальный отступ от профиля")]
    [SerializeField]
    private float minimumTopOffset = 260f;

    [Header("Масштаб на очень низких экранах")]
    [SerializeField]
    private float minimumScale = 0.72f;

    private MainMenuUtilityButton3D[] buttons;

    private void OnEnable()
    {
        Refresh();
    }

    private void Start()
    {
        Refresh();
    }

    private void Update()
    {
        Refresh();
    }

#if UNITY_EDITOR

    private void OnValidate()
    {
        Refresh();
    }

#endif

    public void Refresh()
    {
        ResolveSafeArea();
        ResolveButtons();

        if (safeArea == null ||
            buttons == null ||
            buttons.Length == 0)
        {
            return;
        }

        float safeHeight =
            safeArea.rect.height;

        if (safeHeight <= 1f)
            return;

        // =====================================================
        // СТЕК КНОПОК
        // =====================================================

        float requiredHeight =
            buttonSize.y * 3f +
            buttonStep * 0.4f;

        float availableHeight =
            safeHeight -
            minimumTopOffset -
            30f;

        float scale =
            1f;

        if (
            availableHeight <
            requiredHeight
        )
        {
            scale =
                availableHeight /
                requiredHeight;
        }

        scale =
            Mathf.Clamp(
                scale,
                minimumScale,
                1f
            );

        // =====================================================
        // ПЕРВЫЙ Y
        // =====================================================

        float firstTop =
            safeHeight *
            firstButtonTopRatio;

        firstTop =
            Mathf.Max(
                firstTop,
                minimumTopOffset
            );

        // Если экран настолько низкий,
        // что стек не помещается —
        // слегка сдвигаем его вверх.
        float scaledStackHeight =
            (
                buttonSize.y +
                buttonStep
            ) * 3f *
            scale;

        float maxTop =
            safeHeight -
            scaledStackHeight -
            20f;

        if (maxTop > 0f)
        {
            firstTop =
                Mathf.Min(
                    firstTop,
                    maxTop
                );
        }

        // =====================================================
        // РАССТАНОВКА
        // =====================================================

        for (
            int i = 0;
            i < buttons.Length;
            i++
        )
        {
            MainMenuUtilityButton3D utility =
                buttons[i];

            if (utility == null)
                continue;

            RectTransform rect =
                utility.GetComponent<
                    RectTransform
                >();

            if (rect == null)
                continue;

            // -------------------------------------------------
            // ПЕРЕНОСИМ В SAFE AREA
            // -------------------------------------------------

            if (
                rect.parent !=
                safeArea
            )
            {
                rect.SetParent(
                    safeArea,
                    false
                );
            }

            // -------------------------------------------------
            // TOP LEFT ANCHOR
            // -------------------------------------------------

            rect.anchorMin =
                new Vector2(
                    0f,
                    1f
                );

            rect.anchorMax =
                new Vector2(
                    0f,
                    1f
                );

            rect.pivot =
                new Vector2(
                    0.5f,
                    1f
                );

            rect.sizeDelta =
                buttonSize;

            rect.anchoredPosition =
                new Vector2(
                    leftOffset,
                    -(
                        firstTop +
                        i *
                        buttonStep
                    )
                );

            rect.localScale =
                Vector3.one *
                scale;
        }
    }

    private void ResolveSafeArea()
    {
        if (safeArea != null)
            return;

        GameObject objectByName =
            GameObject.Find(
                "SafeArea"
            );

        if (objectByName != null)
        {
            safeArea =
                objectByName.GetComponent<
                    RectTransform
                >();
        }

        if (safeArea == null)
        {
            Canvas canvas =
                GetComponentInParent<
                    Canvas
                >();

            if (canvas != null)
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
        }
    }

    private void ResolveButtons()
    {
        MainMenuUtilityButton3D[] all =
            FindObjectsByType<
                MainMenuUtilityButton3D
            >(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

        if (
            all == null ||
            all.Length == 0
        )
        {
            buttons =
                null;

            return;
        }

        buttons =
            new MainMenuUtilityButton3D[3];

        foreach (
            MainMenuUtilityButton3D button
            in all
        )
        {
            if (button == null)
                continue;

            switch (button.action)
            {
                case UtilityAction3D.Settings:

                    buttons[0] =
                        button;

                    break;

                case UtilityAction3D.Achievements:

                    buttons[1] =
                        button;

                    break;

                case UtilityAction3D.DailyLogin:

                    buttons[2] =
                        button;

                    break;
            }
        }
    }
}