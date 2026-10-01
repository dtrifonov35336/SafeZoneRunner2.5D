using UnityEngine;
using UnityEngine.UI;

public class TaskCheckmarkUI3D : MonoBehaviour
{
    [Header("Иконка")]
    public Sprite checkmarkSprite;

    [Header("Внешний вид")]
    public Color checkmarkColor = Color.white;

    [Min(8f)]
    public float size = 24f;

    [Min(0f)]
    public float rightInset = 8f;

    [Header("Галочки строк")]
    public Image[] checkmarks = new Image[4];

    [Header("Строки заданий")]
    public RectTransform[] taskRows = new RectTransform[4];

    private void Awake()
    {
        ResolveExistingCheckmarks();
        AdoptSpriteFromExistingImages();
        ApplySpriteToExistingImages();
        HideAll();
    }

    private void Start()
    {
        ResolveExistingCheckmarks();
        AdoptSpriteFromExistingImages();
        ApplySpriteToExistingImages();
        HideAll();
    }

#if UNITY_EDITOR

    private void OnValidate()
    {
        ResolveExistingCheckmarks();
        AdoptSpriteFromExistingImages();

        if (checkmarkSprite != null)
            ApplySpriteToExistingImages();

        ApplyEditorLayout();
    }

#endif

    // =========================================================
    // FIND OBJECTS
    // =========================================================

    private void ResolveExistingCheckmarks()
    {
        if (checkmarks == null || checkmarks.Length != 4)
            checkmarks = new Image[4];

        if (taskRows == null || taskRows.Length != 4)
            taskRows = new RectTransform[4];

        for (int i = 0; i < 4; i++)
        {
            if (checkmarks[i] == null)
            {
                Transform child = transform.Find(
                    "TaskCheckmark_" + i
                );

                if (child != null)
                {
                    checkmarks[i] =
                        child.GetComponent<Image>();
                }
            }

            if (taskRows[i] == null)
            {
                Transform row = transform.Find(
                    "TaskRow_" + i
                );

                if (row != null)
                    taskRows[i] =
                        row.GetComponent<RectTransform>();
            }
        }
    }

    // =========================================================
    // SPRITE
    // =========================================================

    private void AdoptSpriteFromExistingImages()
    {
        if (checkmarkSprite != null)
            return;

        if (checkmarks == null)
            return;

        for (int i = 0; i < checkmarks.Length; i++)
        {
            Image image = checkmarks[i];

            if (image == null)
                continue;

            if (image.sprite != null)
            {
                checkmarkSprite = image.sprite;
                break;
            }
        }
    }

    private void ApplySpriteToExistingImages()
    {
        if (checkmarks == null)
            return;

        for (int i = 0; i < checkmarks.Length; i++)
        {
            Image image = checkmarks[i];

            if (image == null)
                continue;

            if (checkmarkSprite != null)
                image.sprite = checkmarkSprite;

            image.color = checkmarkColor;
            image.preserveAspect = true;
            image.raycastTarget = false;

            RectTransform rect =
                image.rectTransform;

            if (rect != null)
            {
                rect.sizeDelta =
                    new Vector2(size, size);
            }
        }
    }

    // =========================================================
    // LAYOUT
    // =========================================================

    private void ApplyEditorLayout()
    {
        ResolveExistingCheckmarks();

        for (int i = 0; i < 4; i++)
        {
            Image image = checkmarks[i];
            RectTransform row = taskRows[i];

            if (image == null || row == null)
                continue;

            RectTransform checkRect =
                image.rectTransform;

            checkRect.SetParent(
                row,
                false
            );

            checkRect.anchorMin =
                new Vector2(1f, 0.5f);

            checkRect.anchorMax =
                new Vector2(1f, 0.5f);

            checkRect.pivot =
                new Vector2(1f, 0.5f);

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

            image.preserveAspect = true;
            image.raycastTarget = false;
        }
    }

    private void ApplyRuntimeLayout(int index)
    {
        if (
            index < 0 ||
            index >= 4
        )
        {
            return;
        }

        Image image = checkmarks[index];
        RectTransform row = taskRows[index];

        if (image == null || row == null)
            return;

        RectTransform checkRect =
            image.rectTransform;

        if (checkRect.parent != row)
        {
            checkRect.SetParent(
                row,
                false
            );
        }

        checkRect.anchorMin =
            new Vector2(1f, 0.5f);

        checkRect.anchorMax =
            new Vector2(1f, 0.5f);

        checkRect.pivot =
            new Vector2(1f, 0.5f);

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
    // PUBLIC
    // =========================================================

    public void SetTaskRow(
        int index,
        RectTransform row
    )
    {
        if (
            index < 0 ||
            index >= 4
        )
        {
            return;
        }

        taskRows[index] = row;

        if (checkmarks[index] != null)
        {
            checkmarks[index]
                .rectTransform
                .SetParent(
                    row,
                    false
                );

            ApplyRuntimeLayout(index);
        }
    }

    public void SetChecked(
        int index,
        bool completed
    )
    {
        if (
            index < 0 ||
            index >= 4
        )
        {
            return;
        }

        ResolveExistingCheckmarks();

        Image image =
            checkmarks[index];

        if (image == null)
            return;

        ApplyRuntimeLayout(index);

        if (checkmarkSprite == null)
            AdoptSpriteFromExistingImages();

        Sprite sprite =
            checkmarkSprite != null
                ? checkmarkSprite
                : image.sprite;

        if (sprite != null)
            image.sprite = sprite;

        image.color =
            checkmarkColor;

        image.gameObject.SetActive(
            completed &&
            sprite != null
        );
    }

    public void HideAll()
    {
        if (checkmarks == null)
            return;

        for (int i = 0; i < checkmarks.Length; i++)
        {
            if (checkmarks[i] != null)
            {
                checkmarks[i]
                    .gameObject
                    .SetActive(false);
            }
        }
    }
}