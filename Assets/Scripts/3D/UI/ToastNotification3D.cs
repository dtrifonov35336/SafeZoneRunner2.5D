using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ToastNotification : MonoBehaviour
{
    public static ToastNotification Instance { get; private set; }

    [Header("UI")]
    public GameObject toastPanel;
    public TextMeshProUGUI toastText;

    [Header("Настройки")]
    public float defaultDuration = 2.5f;
    public float fadeDuration = 0.3f;

    private CanvasGroup canvasGroup;
    private Coroutine activeRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (toastPanel != null)
        {
            canvasGroup = toastPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = toastPanel.AddComponent<CanvasGroup>();

            canvasGroup.alpha = 0f;
            toastPanel.SetActive(false);
        }
    }

    private void Start()
    {
        // Страховка: если Awake по какой-то причине не выключил панель
        if (toastPanel != null && toastPanel.activeSelf)
        {
            canvasGroup = toastPanel.GetComponent<CanvasGroup>();
            if (canvasGroup != null) canvasGroup.alpha = 0f;
            toastPanel.SetActive(false);
        }
    }

    public void Show(string message, float duration = -1f)
    {
        if (toastPanel == null || toastText == null)
        {
            Debug.Log($"[Toast] {message}");
            return;
        }

        if (duration < 0f) duration = defaultDuration;

        if (activeRoutine != null)
            StopCoroutine(activeRoutine);

        activeRoutine = StartCoroutine(ShowRoutine(message, duration));
    }

    private IEnumerator ShowRoutine(string message, float duration)
    {
        toastText.text = message;
        toastPanel.SetActive(true);

        // Fade in
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Clamp01(t / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 1f;

        // Hold
        yield return new WaitForSecondsRealtime(duration);

        // Fade out
        t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Clamp01(1f - t / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 0f;

        toastPanel.SetActive(false);
        activeRoutine = null;
    }
}