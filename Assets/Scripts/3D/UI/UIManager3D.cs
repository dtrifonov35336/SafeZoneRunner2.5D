using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Старые экранные кнопки")]
    public GameObject touchControlsUI;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Старые кнопки никогда не должны быть видимы.
        HideGameplayControls();
    }

    public void HideGameplayControls()
    {
        if (touchControlsUI != null)
        {
            touchControlsUI.SetActive(false);
        }
    }

    public void ShowGameplayControls()
    {
        // Старые кнопки больше никогда не включаем.
        if (touchControlsUI != null)
        {
            touchControlsUI.SetActive(false);
        }
    }
}