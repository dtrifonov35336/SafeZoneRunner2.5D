using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class SurfaceTextureScroller3D : MonoBehaviour
{
    [Header("Мировая скорость")]
    public float worldSpeed = 15f;

    [Header("Размер поверхности")]
    public float surfaceLength = 320f;

    [Header("Tiling текстуры")]
    public float textureTilingY = 14f;

    [Header("Направление")]
    public bool reverseDirection = false;

    private Renderer surfaceRenderer;
    private Material surfaceMaterial;

    private float offsetY;

    private void Awake()
    {
        surfaceRenderer =
            GetComponent<Renderer>();

        surfaceMaterial =
            surfaceRenderer.material;

        offsetY = 0f;
    }

    private void Update()
    {
        if (ChaseManager.Instance != null &&
            ChaseManager.Instance.IsGameOver())
        {
            return;
        }

        if (surfaceMaterial == null)
            return;

        if (surfaceLength <= 0.01f)
            return;

        float moveSpeed =
            worldSpeed;

        if (ObstacleSpawner3D.Instance != null)
        {
            moveSpeed =
                ObstacleSpawner3D.Instance
                    .CurrentObstacleSpeed;
        }

        float uvSpeed =
            moveSpeed *
            textureTilingY /
            surfaceLength;

        if (reverseDirection)
        {
            offsetY +=
                uvSpeed *
                Time.deltaTime;
        }
        else
        {
            offsetY -=
                uvSpeed *
                Time.deltaTime;
        }

        offsetY =
            Mathf.Repeat(
                offsetY,
                1f
            );

        if (surfaceMaterial.HasProperty(
                "_BaseMap"))
        {
            surfaceMaterial.SetTextureOffset(
                "_BaseMap",
                new Vector2(
                    0f,
                    offsetY
                )
            );
        }

        if (surfaceMaterial.HasProperty(
                "_MainTex"))
        {
            surfaceMaterial.SetTextureOffset(
                "_MainTex",
                new Vector2(
                    0f,
                    offsetY
                )
            );
        }
    }
}