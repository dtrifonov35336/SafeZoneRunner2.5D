using UnityEngine;

[DefaultExecutionOrder(-1000)]
[DisallowMultipleComponent]
public class RunnerLaneSettings3D : MonoBehaviour
{
    public static RunnerLaneSettings3D Instance { get; private set; }

    [Header("Общие полосы движения — настройка для всей игры")]
    [Tooltip("Смещение левой полосы относительно центра дороги. Обычно -1.")]
    public float leftLaneOffset = -1f;

    [Tooltip("Смещение правой полосы относительно центра дороги. Обычно 1.")]
    public float rightLaneOffset = 1f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError(
                "[RunnerLaneSettings3D] В сцене должен быть только один компонент настроек полос.",
                this
            );
            Destroy(this);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private static RunnerLaneSettings3D GetSettings()
    {
        if (Instance == null)
            Instance = Object.FindFirstObjectByType<RunnerLaneSettings3D>();

        return Instance;
    }

    public static float GetCenterX()
    {
        GameObject centerObject = GameObject.Find("RoadCenterMarking");

        if (centerObject != null)
            return centerObject.transform.position.x;

        RoadDashedLine line = Object.FindFirstObjectByType<RoadDashedLine>();

        if (line != null)
            return line.transform.position.x;

        return 0f;
    }

    public static float GetLaneX(int index)
    {
        float centerX = GetCenterX();
        RunnerLaneSettings3D settings = GetSettings();

        if (settings == null)
            return centerX + (index <= 0 ? -1f : 1f);

        return centerX + (
            index <= 0
                ? settings.leftLaneOffset
                : settings.rightLaneOffset
        );
    }

    public static float[] GetLanePositions()
    {
        float centerX = GetCenterX();
        RunnerLaneSettings3D settings = GetSettings();

        if (settings == null)
            return new float[] { centerX - 1f, centerX + 1f };

        return new float[]
        {
            centerX + settings.leftLaneOffset,
            centerX + settings.rightLaneOffset
        };
    }
}
