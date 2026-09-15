using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(SpriteRenderer))]
public class AspectFitter : MonoBehaviour
{
    public enum FitMode
    {
        None,
        CoverWidth,    // по ширине
        CoverHeight,   // по высоте
        Cover,         // заполнить экран целиком (гарантированно больше экрана)
        Contain        // вписать целиком
    }

    [Header("Режим")]
    public FitMode mode = FitMode.Cover;

    [Header("Доп. отступ (в юнитах)")]
    public float extraPadding = 0.5f;

    [Header("Позиция (от центра камеры)")]
    [Tooltip("Смещение от центра экрана. (0,0) = центр")]
    public Vector2 offset = Vector2.zero;

    [Header("Отладка")]
    public bool debugLog = false;

    private SpriteRenderer sr;
    private Camera cam;
    private float lastAspect = -1f;

    private void OnEnable()
    {
        sr = GetComponent<SpriteRenderer>();
        cam = Camera.main;
        Apply();
    }

    private void Start() => Apply();

    private void Update()
    {
        if (cam == null) cam = Camera.main;
        if (cam == null) return;
        if (Screen.width <= 0 || Screen.height <= 0) return;

        if (Mathf.Abs(cam.aspect - lastAspect) > 0.001f)
            Apply();
    }

    private void Apply()
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        if (sr == null || sr.sprite == null) return;

        if (cam == null) cam = Camera.main;
        if (cam == null) return;

        float orthoSize = cam.orthographicSize;
        if (orthoSize <= 0f || float.IsNaN(orthoSize)) return;

        Vector2 spriteSize = sr.sprite.bounds.size;
        if (spriteSize.x <= 0.001f || spriteSize.y <= 0.001f) return;

        float aspect = cam.aspect;
        if (aspect <= 0f || float.IsNaN(aspect)) return;

        lastAspect = aspect;

        float screenH = orthoSize * 2f;
        float screenW = screenH * aspect;

        float targetW = screenW + extraPadding * 2f;
        float targetH = screenH + extraPadding * 2f;

        float fitX = targetW / spriteSize.x;
        float fitY = targetH / spriteSize.y;

        float finalScale = 1f;
        switch (mode)
        {
            case FitMode.None: return;
            case FitMode.CoverWidth: finalScale = fitX; break;
            case FitMode.CoverHeight: finalScale = fitY; break;
            case FitMode.Cover: finalScale = Mathf.Max(fitX, fitY); break;
            case FitMode.Contain: finalScale = Mathf.Min(fitX, fitY); break;
        }

        if (float.IsNaN(finalScale) || float.IsInfinity(finalScale)) return;

        transform.localScale = new Vector3(finalScale, finalScale, 1f);

        // Позиция — центр камеры + offset
        Vector3 camPos = cam.transform.position;
        transform.position = new Vector3(
            camPos.x + offset.x,
            camPos.y + offset.y,
            transform.position.z
        );

        if (debugLog)
            Debug.Log($"[AspectFitter] {name}: mode={mode}, aspect={aspect:F2}, " +
                      $"screen={screenW:F2}×{screenH:F2}, sprite={spriteSize.x:F2}×{spriteSize.y:F2}, " +
                      $"scale={finalScale:F2}");
    }
}