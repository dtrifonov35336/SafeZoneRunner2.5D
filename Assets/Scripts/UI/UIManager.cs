using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Скрывать при панелях")]
    public GameObject touchControlsUI;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void HideGameplayControls()
    {
        if (touchControlsUI != null) touchControlsUI.SetActive(false);
    }

    public void ShowGameplayControls()
    {
        if (touchControlsUI != null) touchControlsUI.SetActive(true);
    }
}