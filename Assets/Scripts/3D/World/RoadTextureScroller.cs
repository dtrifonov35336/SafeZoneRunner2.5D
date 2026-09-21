using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class RoadTextureScroller : MonoBehaviour
{
    [Header("Мировая скорость")]
    public float worldSpeed = 15f;

    [Header("Параметры дороги")]
    public float roadLength = 300f;
    public float textureTilingY = 14f;

    private Renderer roadRenderer;
    private Material roadMaterial;

    private float offsetY;

    private void Awake()
    {
        roadRenderer = GetComponent<Renderer>();

        // Создаём runtime-копию материала.
        // Исходный Road_2.5D_Surface.mat не изменяется.
        roadMaterial = roadRenderer.material;

        offsetY = 0f;
    }

    private void Update()
    {
        if (ChaseManager.Instance != null &&
            ChaseManager.Instance.IsGameOver())
            return;

        if (roadMaterial == null)
            return;

        // Сколько полных повторов текстуры соответствует
        // скорости движения мира.
        float uvSpeed =
            worldSpeed * textureTilingY / roadLength;

        offsetY -= uvSpeed * Time.deltaTime;

        offsetY = Mathf.Repeat(offsetY, 1f);

        roadMaterial.SetTextureOffset(
            "_BaseMap",
            new Vector2(0f, offsetY)
        );
    }
}