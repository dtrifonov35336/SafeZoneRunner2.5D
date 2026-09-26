using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RunProgressUI3D : MonoBehaviour
{
    [Header("Run Manager")]
    public RunManager runManager;

    [Header("UI")]
    public GameObject root;
    public Image fill;
    public TextMeshProUGUI label;

    [Header("Настройки")]
    [Range(0f, 1f)]
    public float startFill = 0f;

    private void Awake()
    {
        if (runManager == null)
            runManager =
                FindFirstObjectByType<RunManager>();
    }

    private void Start()
    {
        UpdateVisual();
    }

    private void Update()
    {
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (runManager == null)
            return;

        bool visible =
            !runManager.infiniteRun;

        if (root != null &&
            root.activeSelf != visible)
        {
            root.SetActive(visible);
        }

        if (!visible)
            return;

        float progress =
            runManager.GetRunProgress();

        if (fill != null)
        {
            fill.fillAmount =
                Mathf.Clamp01(progress);
        }
    }
}