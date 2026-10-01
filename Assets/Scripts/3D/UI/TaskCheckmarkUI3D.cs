using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TaskCheckmarkUI3D : MonoBehaviour
{
    [Header("Checkmark")]
    [SerializeField] private Sprite checkmarkSprite;
    [SerializeField] private Color checkmarkColor = Color.white;

    [Min(8f)]
    [SerializeField] private float size = 24f;

    [SerializeField] private float rightInset = 8f;

    [Header("Rows")]
    [SerializeField]
    private RectTransform[] taskRows =
        new RectTransform[4];

    [SerializeField]
    private Image[] checkmarks =
        new Image[4];

    private const int RowCount = 4;

    private void Awake()
    {
        ResolveAll();
        ApplyAppearance();
        HideAll();
    }

    private void Start()
    {
        ResolveAll();
        ApplyAppearance();
        LayoutFixed();
        HideAll();
    }

    // =========================================================
    // RESOLVE
    // =========================================================

    private void ResolveAll()
    {
        ResolveRows();
        ResolveCheckmarks();
    }

    private void ResolveRows()
    {
        if (
            taskRows == null ||
            taskRows.Length != RowCount
        )
        {
            taskRows =
                new RectTransform[RowCount];
        }

        for (int i = 0; i < RowCount; i++)
        {
            if (taskRows[i] != null)
                continue;

            string rowName =
                i == 0
                    ? "TaskText"
                    : "TaskText_" + i;

            Transform row =
                transform.Find(rowName);

            if (row == null)
                continue;

            taskRows[i] =
                row.GetComponent<RectTransform>();
        }
    }

    private void ResolveCheckmarks()
    {
        if (
            checkmarks == null ||
            checkmarks.Length != RowCount
        )
        {
            checkmarks =
                new Image[RowCount];
        }

        for (int i = 0; i < RowCount; i++)
        {
            if (checkmarks[i] != null)
                continue;

            // Сначала ищем внутри соответствующей строки.
            if (taskRows != null &&
                i < taskRows.Length &&
                taskRows[i] != null)
            {
                Transform child =
                    taskRows[i].Find(
                        "TaskCheckmark_" + i
                    );

                if (child != null)
                {
                    checkmarks[i] =
                        child.GetComponent<Image>();

                    if (checkmarks[i] != null)
                        continue;
                }
            }

            // Старый вариант: галочка лежит прямо в TaskBox.
            Transform legacy =
                transform.Find(
                    "TaskCheckmark_" + i
                );

            if (legacy != null)
            {
                checkmarks[i] =
                    legacy.GetComponent<Image>();
            }
        }
    }

    // =========================================================
    // APPEARANCE
    // =========================================================

    private void ApplyAppearance()
    {
        ResolveAll();

        for (int i = 0; i < RowCount; i++)
        {
            Image image =
                checkmarks[i];

            if (image == null)
                continue;

            if (checkmarkSprite != null)
            {
                image.sprite =
                    checkmarkSprite;
            }

            image.color =
                checkmarkColor;

            image.preserveAspect =
                true;

            image.raycastTarget =
                false;

            RectTransform rect =
                image.rectTransform;

            rect.sizeDelta =
                new Vector2(
                    size,
                    size
                );

            rect.localScale =
                Vector3.one;
        }
    }

    // =========================================================
    // LAYOUT
    // =========================================================

    public void LayoutFixed()
    {
        ResolveAll();

        for (int i = 0; i < RowCount; i++)
        {
            RectTransform row =
                taskRows[i];

            if (row == null)
                continue;

            ConfigureRow(
                row,
                i
            );
        }
    }

    private void ConfigureRow(
        RectTransform row,
        int index
    )
    {
        const float rowTop = 54f;
        const float rowHeight = 32f;
        const float leftInset = 12f;
        const float rightTextSpace = 42f;

        row.anchorMin =
            new Vector2(
                0f,
                1f
            );

        row.anchorMax =
            new Vector2(
                1f,
                1f
            );

        row.pivot =
            new Vector2(
                0f,
                1f
            );

        row.anchoredPosition =
            new Vector2(
                leftInset,
                -rowTop -
                index * rowHeight
            );

        row.sizeDelta =
            new Vector2(
                -leftInset -
                rightTextSpace,
                rowHeight
            );

        row.localScale =
            Vector3.one;

        TextMeshProUGUI text =
            row.GetComponent<
                TextMeshProUGUI
            >();

        if (text != null)
        {
            text.alignment =
                TextAlignmentOptions.Left;

            text.enableAutoSizing =
                false;

            text.textWrappingMode =
                TextWrappingModes.NoWrap;

            text.overflowMode =
                TextOverflowModes.Ellipsis;

            text.raycastTarget =
                false;

            text.margin =
                new Vector4(
                    0f,
                    0f,
                    0f,
                    0f
                );
        }

        Image image =
            checkmarks[index];

        if (image == null)
            return;

        // Перемещаем галочку внутрь строки.
        if (image.transform.parent != row)
        {
            image.transform.SetParent(
                row,
                false
            );
        }

        RectTransform checkRect =
            image.rectTransform;

        checkRect.anchorMin =
            new Vector2(
                1f,
                0.5f
            );

        checkRect.anchorMax =
            new Vector2(
                1f,
                0.5f
            );

        checkRect.pivot =
            new Vector2(
                1f,
                0.5f
            );

        checkRect.sizeDelta =
            new Vector2(
                size,
                size
            );

        checkRect.anchoredPosition =
            new Vector2(
                -rightInset,
                0f
            );

        checkRect.localScale =
            Vector3.one;
    }

    // =========================================================
    // TASK
    // =========================================================

    public void SetTask(
        int index,
        string text,
        bool completed,
        bool danger = false
    )
    {
        if (
            index < 0 ||
            index >= RowCount
        )
        {
            return;
        }

        ResolveAll();

        TextMeshProUGUI rowText =
            taskRows[index] != null
                ? taskRows[index]
                    .GetComponent<
                        TextMeshProUGUI
                    >()
                : null;

        if (rowText != null)
        {
            rowText.text =
                text;

            rowText.fontSize =
                index == 0
                    ? 20f
                    : 18f;

            rowText.enableAutoSizing =
                false;

            rowText.textWrappingMode =
                TextWrappingModes.NoWrap;

            rowText.overflowMode =
                TextOverflowModes.Ellipsis;

            rowText.alignment =
                TextAlignmentOptions.Left;

            rowText.raycastTarget =
                false;

            if (completed)
            {
                rowText.color =
                    new Color32(
                        152,
                        184,
                        142,
                        255
                    );
            }
            else if (danger)
            {
                rowText.color =
                    new Color32(
                        197,
                        111,
                        111,
                        255
                    );
            }
            else if (index == 0)
            {
                rowText.color =
                    new Color32(
                        217,
                        213,
                        203,
                        255
                    );
            }
            else
            {
                rowText.color =
                    new Color32(
                        169,
                        173,
                        169,
                        255
                    );
            }
        }

        SetChecked(
            index,
            completed
        );
    }

    // =========================================================
    // CHECKMARK
    // =========================================================

    public void SetChecked(
        int index,
        bool completed
    )
    {
        if (
            index < 0 ||
            index >= RowCount
        )
        {
            return;
        }

        ResolveAll();

        Image image =
            checkmarks[index];

        if (image == null)
            return;

        if (checkmarkSprite != null)
        {
            image.sprite =
                checkmarkSprite;
        }

        image.color =
            checkmarkColor;

        image.gameObject.SetActive(
            completed &&
            image.sprite != null
        );
    }

    public void HideAll()
    {
        ResolveCheckmarks();

        for (int i = 0; i < RowCount; i++)
        {
            if (checkmarks[i] != null)
            {
                checkmarks[i]
                    .gameObject
                    .SetActive(false);
            }
        }
    }

    // =========================================================
    // EDITOR SUPPORT
    // =========================================================

    public void Configure(
        RectTransform[] rows,
        Image[] marks
    )
    {
        taskRows =
            new RectTransform[RowCount];

        checkmarks =
            new Image[RowCount];

        for (int i = 0; i < RowCount; i++)
        {
            if (
                rows != null &&
                i < rows.Length
            )
            {
                taskRows[i] =
                    rows[i];
            }

            if (
                marks != null &&
                i < marks.Length
            )
            {
                checkmarks[i] =
                    marks[i];
            }
        }

        ApplyAppearance();
        LayoutFixed();
    }

    public RectTransform[] GetTaskRows()
    {
        ResolveRows();
        return taskRows;
    }
}