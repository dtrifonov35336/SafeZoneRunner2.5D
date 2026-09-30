using UnityEngine;
using UnityEngine.UI;

public class TaskCheckmarkUI3D : MonoBehaviour
{
    [Header("Иконка")]
    public Sprite checkmarkSprite;

    [Header("Внешний вид")]
    public Color checkmarkColor =
        Color.white;

    [Min(8f)]
    public float size = 24f;

    [Header("Позиция")]
    [Min(0f)]
    public float rightInset = 8f;

    public float firstLineY = -66f;

    public float lineStep = 34f;

    [Header("Объекты галочек")]
    public Image[] checkmarks =
        new Image[4];

    private void Awake()
    {
        ResolveExistingCheckmarks();
        ApplyLayout();
        HideAll();
    }

    private void Start()
    {
        ResolveExistingCheckmarks();
        ApplyLayout();
        HideAll();
    }

#if UNITY_EDITOR

    private void OnValidate()
    {
        ResolveExistingCheckmarks();
        ApplyLayout();
        ApplySprite();
    }

#endif

    // =========================================================
    // FIND EXISTING OBJECTS
    // =========================================================

    private void ResolveExistingCheckmarks()
    {
        if (
            checkmarks == null ||
            checkmarks.Length != 4
        )
        {
            checkmarks =
                new Image[4];
        }

        for (
            int i = 0;
            i < 4;
            i++
        )
        {
            if (
                checkmarks[i] != null
            )
            {
                continue;
            }

            Transform child =
                transform.Find(
                    "TaskCheckmark_" +
                    i
                );

            if (child == null)
                continue;

            checkmarks[i] =
                child.GetComponent<
                    Image
                >();
        }
    }

    // =========================================================
    // LAYOUT
    // =========================================================

    private void ApplyLayout()
    {
        if (
            checkmarks == null
        )
        {
            return;
        }

        for (
            int i = 0;
            i < checkmarks.Length;
            i++
        )
        {
            Image image =
                checkmarks[i];

            if (image == null)
                continue;

            RectTransform rect =
                image.rectTransform;

            if (rect == null)
                continue;

            rect.anchorMin =
                new Vector2(
                    1f,
                    1f
                );

            rect.anchorMax =
                new Vector2(
                    1f,
                    1f
                );

            rect.pivot =
                new Vector2(
                    1f,
                    0.5f
                );

            rect.sizeDelta =
                new Vector2(
                    size,
                    size
                );

            rect.anchoredPosition =
                new Vector2(
                    -rightInset,
                    firstLineY -
                    i * lineStep
                );

            rect.localScale =
                Vector3.one;

            image.preserveAspect =
                true;

            image.raycastTarget =
                false;
        }
    }

    // =========================================================
    // SPRITE
    // =========================================================

    private void ApplySprite()
    {
        if (
            checkmarks == null
        )
        {
            return;
        }

        for (
            int i = 0;
            i < checkmarks.Length;
            i++
        )
        {
            if (
                checkmarks[i] == null
            )
            {
                continue;
            }

            checkmarks[i].sprite =
                checkmarkSprite;

            checkmarks[i].color =
                checkmarkColor;
        }
    }

    // =========================================================
    // RUNTIME
    // =========================================================

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

        image.sprite =
            checkmarkSprite;

        image.color =
            checkmarkColor;

        bool visible =
            completed &&
            checkmarkSprite != null;

        image.gameObject.SetActive(
            visible
        );
    }

    public void HideAll()
    {
        if (
            checkmarks == null
        )
        {
            return;
        }

        for (
            int i = 0;
            i < checkmarks.Length;
            i++
        )
        {
            if (
                checkmarks[i] != null
            )
            {
                checkmarks[i]
                    .gameObject
                    .SetActive(
                        false
                    );
            }
        }
    }
}