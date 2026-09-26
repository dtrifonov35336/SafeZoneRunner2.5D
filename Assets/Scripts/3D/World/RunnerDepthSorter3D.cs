using UnityEngine;

public class RunnerDepthSorter3D : MonoBehaviour
{
    [Header("Глубина")]
    [Tooltip("Чем меньше Z, тем ближе объект к игроку.")]
    public float zMultiplier = 1000f;

    [Tooltip("Базовый Sorting Order.")]
    public int sortingBase = 100000;

    private SpriteRenderer[] spriteRenderers;
    private int[] originalOrders;

    // Общий Sorting Layer для всех дорожных объектов.
    private static int sharedSortingLayerId = -1;
    private static bool sharedLayerInitialized = false;

    private void Awake()
    {
        spriteRenderers =
            GetComponentsInChildren<SpriteRenderer>(
                true
            );

        originalOrders =
            new int[spriteRenderers.Length];

        for (int i = 0;
             i < spriteRenderers.Length;
             i++)
        {
            if (spriteRenderers[i] != null)
            {
                originalOrders[i] =
                    spriteRenderers[i].sortingOrder;
            }
        }

        TryInitializeSharedLayer();
    }

    private void LateUpdate()
    {
        TryInitializeSharedLayer();

        if (spriteRenderers == null)
            return;

        // Меньший Z = ближе к игроку = выше Sorting Order.
        int depthOrder =
            sortingBase -
            Mathf.RoundToInt(
                transform.position.z *
                zMultiplier
            );

        for (int i = 0;
             i < spriteRenderers.Length;
             i++)
        {
            SpriteRenderer sr =
                spriteRenderers[i];

            if (sr == null)
                continue;

            // Все дорожные объекты находятся
            // на одном Sorting Layer.
            if (sharedLayerInitialized)
            {
                sr.sortingLayerID =
                    sharedSortingLayerId;
            }

            sr.sortingOrder =
                originalOrders[i] +
                depthOrder;
        }
    }

    private void TryInitializeSharedLayer()
    {
        if (sharedLayerInitialized)
            return;

        // Ищем любое уже созданное препятствие.
        ObstacleMover3D[] obstacles =
            FindObjectsByType<ObstacleMover3D>(
                FindObjectsSortMode.None
            );

        foreach (ObstacleMover3D obstacle in obstacles)
        {
            if (obstacle == null)
                continue;

            SpriteRenderer renderer =
                obstacle.GetComponentInChildren<SpriteRenderer>(
                    true
                );

            if (renderer == null)
                continue;

            sharedSortingLayerId =
                renderer.sortingLayerID;

            sharedLayerInitialized =
                true;

            return;
        }
    }
}