using UnityEngine;
using UnityEngine.Rendering;

public class RunnerDepthSorter3D : MonoBehaviour
{
    [Header("Сортировка глубины")]
    [Tooltip("Чем меньше Z, тем объект ближе к игроку.")]
    public float zMultiplier = 100f;

    [Tooltip("Небольшое влияние высоты Y на порядок.")]
    public float yMultiplier = 5f;

    [Tooltip("Базовое значение Sorting Order.")]
    public int sortingBase = 10000;

    private SpriteRenderer[] spriteRenderers;
    private int[] originalOrders;

    private SortingGroup sortingGroup;
    private int originalGroupOrder;

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

        sortingGroup =
            GetComponent<SortingGroup>();

        if (sortingGroup != null)
        {
            originalGroupOrder =
                sortingGroup.sortingOrder;
        }
    }

    private void LateUpdate()
    {
        int depthOrder =
            sortingBase
            - Mathf.RoundToInt(
                transform.position.z *
                zMultiplier
            )
            + Mathf.RoundToInt(
                transform.position.y *
                yMultiplier
            );

        // Если на корне есть SortingGroup,
        // сортируем всю группу целиком.
        if (sortingGroup != null)
        {
            sortingGroup.sortingOrder =
                originalGroupOrder +
                depthOrder;

            return;
        }

        // Обычные SpriteRenderer.
        for (int i = 0;
             i < spriteRenderers.Length;
             i++)
        {
            SpriteRenderer sr =
                spriteRenderers[i];

            if (sr == null)
                continue;

            sr.sortingOrder =
                originalOrders[i] +
                depthOrder;
        }
    }
}