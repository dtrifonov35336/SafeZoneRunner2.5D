using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Вешается на любую кнопку «+» в TopBar.
/// По клику загружает указанную сцену (по умолчанию — Shop).
/// </summary>
[RequireComponent(typeof(Button))]
public class PlusButtonHandler : MonoBehaviour
{
    [Header("Сцена назначения")]
    public string targetScene = "Shop";

    private void Start()
    {
        var btn = GetComponent<Button>();
        if (btn == null) return;

        btn.onClick.RemoveListener(OnClick);
        btn.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        // Если уже в Shop — ничего не делаем
        if (SceneManager.GetActiveScene().name == targetScene)
        {
            Debug.Log("[Plus] Уже в магазине");
            return;
        }

        Debug.Log($"[Plus] Переход в {targetScene}");
        SceneManager.LoadScene(targetScene);
    }
}