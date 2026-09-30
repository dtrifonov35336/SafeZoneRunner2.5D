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

    private readonly Image[] checkmarks =
        new Image[4];

    private bool initialized;

    private void Awake()
    {
        EnsureCheckmarks();
    }

    public void SetChecked(
        int index,
        bool completed)
    {
        if (index < 0 ||
            index >= checkmarks.Length)
        {
            return;
        }

        EnsureCheckmarks();

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
        EnsureCheckmarks();

        for (int i = 0;
            i < checkmarks.Length;
            i++)
        {
            if (checkmarks[i] != null)
            {
                checkmarks[i]
                    .gameObject
                    .SetActive(false);
            }
        }
    }

    private void EnsureCheckmarks()
    {
        if (initialized &&
            checkmarks[0] != null)
        {
            return;
        }

        Transform taskBox =
            transform;

        if (taskBox == null)
            return;

        RectTransform taskBoxRect =
            GetComponent<RectTransform>();

        if (taskBoxRect == null)
            return;

        for (int i = 0;
            i < checkmarks.Length;
            i++)
        {
            string objectName =
                "TaskCheckmark_" +
                i;

            Transform existing =
                taskBox.Find(
                    objectName
                );

            GameObject go;

            if (existing != null)
            {
                go =
                    existing.gameObject;
            }
            else
            {
                go =
                    new GameObject(
                        objectName,
                        typeof(RectTransform),
                        typeof(Image)
                    );

                go.transform.SetParent(
                    taskBox,
                    false
                );
            }

            Image image =
                go.GetComponent<Image>();

            if (image == null)
            {
                image =
                    go.AddComponent<Image>();
            }

            RectTransform rect =
                go.GetComponent<
                    RectTransform
                >();

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

            image.sprite =
                checkmarkSprite;

            image.color =
                checkmarkColor;

            image.preserveAspect =
                true;

            image.raycastTarget =
                false;

            image.gameObject.SetActive(
                false
            );

            checkmarks[i] =
                image;
        }

        initialized =
            true;
    }
}