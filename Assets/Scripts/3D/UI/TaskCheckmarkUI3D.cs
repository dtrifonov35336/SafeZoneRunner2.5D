using UnityEngine;
using UnityEngine.UI;

public class TaskCheckmarkUI3D : MonoBehaviour
{
    [SerializeField] private Sprite checkmarkSprite;
    [SerializeField] private Color checkmarkColor = Color.white;

    [Min(8f)]
    [SerializeField] private float size = 24f;

    [SerializeField] private float rightInset = 8f;

    [SerializeField]
    private Image[] checkmarks = new Image[4];

    private void Awake()
    {
        ResolveCheckmarks();
        ApplyAppearance();
        HideAll();
    }

    private void Start()
    {
        ResolveCheckmarks();
        ApplyAppearance();
        HideAll();
        LayoutFixed();
    }

    private void ResolveCheckmarks()
    {
        if (
            checkmarks == null ||
            checkmarks.Length != 4
        )
        {
            checkmarks = new Image[4];
        }

        for (int i = 0; i < 4; i++)
        {
            if (checkmarks[i] != null)
                continue;

            Transform child =
                transform.Find(
                    "TaskCheckmark_" + i
                );

            if (child != null)
            {
                checkmarks[i] =
                    child.GetComponent<Image>();
            }
        }
    }

    private void ApplyAppearance()
    {
        for (int i = 0; i < 4; i++)
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

            rect.anchorMin =
                new Vector2(1f, 1f);

            rect.anchorMax =
                new Vector2(1f, 1f);

            rect.pivot =
                new Vector2(1f, 0.5f);

            rect.sizeDelta =
                new Vector2(
                    size,
                    size
                );

            rect.localScale =
                Vector3.one;
        }
    }

    public void LayoutFixed()
    {
        ResolveCheckmarks();

        // Фиксированное положение относительно TaskBox.
        // 4 строки: 0 = главная задача.
        float[] y =
        {
            -72f,
            -106f,
            -140f,
            -174f
        };

        for (int i = 0; i < 4; i++)
        {
            if (checkmarks[i] == null)
                continue;

            RectTransform rect =
                checkmarks[i].rectTransform;

            rect.anchorMin =
                new Vector2(1f, 1f);

            rect.anchorMax =
                new Vector2(1f, 1f);

            rect.pivot =
                new Vector2(1f, 0.5f);

            rect.sizeDelta =
                new Vector2(
                    size,
                    size
                );

            rect.anchoredPosition =
                new Vector2(
                    -rightInset,
                    y[i]
                );

            rect.localScale =
                Vector3.one;
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

        ResolveCheckmarks();

        Image image =
            checkmarks[index];

        if (image == null)
            return;

        if (checkmarkSprite != null)
            image.sprite = checkmarkSprite;

        image.color = checkmarkColor;

        image.gameObject.SetActive(
            completed &&
            image.sprite != null
        );
    }

    public void HideAll()
    {
        ResolveCheckmarks();

        for (int i = 0; i < 4; i++)
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