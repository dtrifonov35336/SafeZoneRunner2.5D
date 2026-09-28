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
            achievementsUI = GetComponentInParent<AchievementsUI3D>(true);

            if (achievementsUI == null)
            {
                achievementsUI = FindAnyAchievementsUI();
            }
        }

        if (button != null)
            button.onClick.AddListener(OpenAchievements);
    }

    private void OnEnable()
    {
        RefreshBadge();
    }

    public void OpenAchievements()
    {
        if (achievementsUI == null)
        {
            achievementsUI = FindAnyAchievementsUI();
        }

        if (achievementsUI == null)
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[AchievementsButton] Canvas не найден.");
                return;
            }

            GameObject uiObject = new GameObject(
                "AchievementsUI",
                typeof(RectTransform),
                typeof(AchievementsUI3D));
            uiObject.transform.SetParent(canvas.transform, false);
            achievementsUI = uiObject.GetComponent<AchievementsUI3D>();
        }

        achievementsUI.Open();
    }

    private AchievementsUI3D FindAnyAchievementsUI()
    {
        AchievementsUI3D[] all = FindObjectsByType<AchievementsUI3D>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        return all != null && all.Length > 0 ? all[0] : null;
    }

    public void RefreshBadge()
    {
        bool hasUnclaimed = AchievementSystem3D.HasUnclaimed();

        if (badge != null)
            badge.SetActive(hasUnclaimed);

        if (badgeText != null)
            badgeText.text = "!";
    }
}
