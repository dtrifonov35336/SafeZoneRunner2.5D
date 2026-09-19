using UnityEngine;
using System.Collections.Generic;

public class RoadMarkingScroller : MonoBehaviour
{
    [Header("Разметка")]
    public GameObject markingPrefab;
    [Tooltip("Сколько штрихов одновременно в сцене")]
    public int markingCount = 8;

    [Header("Движение")]
    public float scrollSpeed = 2f;
    public float spawnY = 1.3f;
    public float despawnY = -8f;

    [Header("Перспектива (масштаб)")]
    public float scaleAtTop = 0.1f;
    public float scaleAtBottom = 0.3f;

    [Header("Смещение по X (центровка)")]
    [Tooltip("Сдвинь, если штрих левее/правее центра")]
    public float xOffset = 0f;

    private List<Transform> markings = new List<Transform>();
    private float timer = 0f;

    void Start()
    {
        if (markingPrefab == null)
        {
            Debug.LogError("[RoadMarking] Marking Prefab пустой!");
            return;
        }

        for (int i = 0; i < markingCount; i++)
        {
            GameObject m = Instantiate(markingPrefab, transform);
            m.name = "Marking_" + i;
            m.transform.localPosition = Vector3.zero;
            m.transform.localScale = Vector3.one;
            markings.Add(m.transform);
        }

        Debug.Log($"[RoadMarking] Создано {markings.Count} штрихов");
    }

    void Update()
    {
        if (markings.Count == 0) return;

        // Проверка на конец игры
        if (ChaseManager.Instance != null && ChaseManager.Instance.IsGameOver())
            return;

        timer += Time.deltaTime;

        float totalPath = spawnY - despawnY;
        if (totalPath <= 0.01f) return;

        // Общий прогресс для всех штрихов
        float baseProgress = (timer * scrollSpeed) / totalPath;

        for (int i = 0; i < markings.Count; i++)
        {
            if (markings[i] == null) continue;

            // Каждый штрих сдвинут на 1/Count от базового прогресса
            float progress = baseProgress + (float)i / markingCount;
            progress = progress % 1f; // зацикливание

            float y = Mathf.Lerp(spawnY, despawnY, progress);
            float scale = Mathf.Lerp(scaleAtTop, scaleAtBottom, progress);

            markings[i].localPosition = new Vector3(xOffset, y, 0);
            markings[i].localScale = new Vector3(scale, scale, 1);
        }
    }
}