using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AchievementsMenuButton3D : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private GameObject badge;
    [SerializeField] private TextMeshProUGUI badgeText;
    [SerializeField] private AchievementsUI3D achievementsUI;

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();

        if (achievementsUI == null)
        {
            achievementsUI =
                GetComponentInParent<AchievementsUI3D>(true);

            if (achievementsUI == null)
                achievementsUI = FindAnyAchievementsUI();
        }

        EnsureBadge();

        if (button != null)
        {
            button.onClick.RemoveListener(OpenAchievements);
            button.onClick.AddListener(OpenAchievements);
        }
    }

    private void OnEnable()
    {
        RefreshBadge();
    }

    public void OpenAchievements()
    {
        if (achievementsUI == null)
            achievementsUI = FindAnyAchievementsUI();

        if (achievementsUI == null)
        {
            Canvas canvas =
                FindFirstObjectByType<Canvas>();

            if (canvas == null)
            {
                Debug.LogError(
                    "[AchievementsButton] Canvas не найден.");
                return;
            }

            GameObject uiObject =
                new GameObject(
                    "AchievementsUI",
                    typeof(RectTransform),
                    typeof(AchievementsUI3D));

            uiObject.transform.SetParent(
                canvas.transform,
                false);

            achievementsUI =
                uiObject.GetComponent<AchievementsUI3D>();
        }

        achievementsUI.Open();
    }

    private AchievementsUI3D FindAnyAchievementsUI()
    {
        AchievementsUI3D[] all =
            FindObjectsByType<AchievementsUI3D>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

        return all != null && all.Length > 0
            ? all[0]
            : null;
    }

    public void RefreshBadge()
    {
        EnsureBadge();

        bool hasUnclaimed =
            AchievementSystem3D.HasUnclaimed();

        if (badge != null)
            badge.SetActive(hasUnclaimed);

        if (badgeText != null)
            badgeText.text = "!";
    }

    private void EnsureBadge()
    {
        if (badge == null)
        {
            Transform existing =
                transform.Find("Badge");

            if (existing != null)
            {
                badge = existing.gameObject;
            }
        }

        if (badge == null)
        {
            GameObject badgeObject =
                new GameObject(
                    "Badge",
                    typeof(RectTransform),
                    typeof(Image));

            badgeObject.transform.SetParent(
                transform,
                false);

            badge = badgeObject;

            RectTransform badgeRect =
                badgeObject.GetComponent<RectTransform>();

            badgeRect.anchorMin =
                new Vector2(1f, 1f);

            badgeRect.anchorMax =
                new Vector2(1f, 1f);

            badgeRect.pivot =
                new Vector2(1f, 1f);

            badgeRect.anchoredPosition =
                new Vector2(-2f, -2f);

            badgeRect.sizeDelta =
                new Vector2(26f, 26f);
        }

        Image image =
            badge.GetComponent<Image>();

        if (image != null)
        {
            image.color =
                new Color(
                    0.92f,
                    0.08f,
                    0.08f,
                    1f);

            image.raycastTarget = false;
        }

        if (badgeText == null)
        {
            Transform textTransform =
                badge.transform.Find("Text");

            if (textTransform != null)
            {
                badgeText =
                    textTransform.GetComponent<TextMeshProUGUI>();
            }
        }

        if (badgeText == null)
        {
            GameObject textObject =
                new GameObject(
                    "Text",
                    typeof(RectTransform),
                    typeof(TextMeshProUGUI));

            textObject.transform.SetParent(
                badge.transform,
                false);

            badgeText =
                textObject.GetComponent<TextMeshProUGUI>();
        }

        RectTransform textRect =
            badgeText.rectTransform;

        textRect.anchorMin =
            Vector2.zero;

        textRect.anchorMax =
            Vector2.one;

        textRect.offsetMin =
            Vector2.zero;

        textRect.offsetMax =
            Vector2.zero;

        badgeText.text = "!";
        badgeText.fontSize = 17f;
        badgeText.fontStyle =
            FontStyles.Bold;
        badgeText.alignment =
            TextAlignmentOptions.Center;
        badgeText.color = Color.white;
        badgeText.raycastTarget = false;
        badgeText.textWrappingMode =
            TextWrappingModes.NoWrap;
    }
}