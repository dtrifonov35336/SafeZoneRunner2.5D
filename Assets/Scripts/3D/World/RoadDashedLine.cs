using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class RoadDashedLine : MonoBehaviour
{
    [Header("Длина дороги")]
    [Min(1f)]
    public float roadLength = 300f;

    [Header("Разметка")]
    [Min(0.1f)]
    public float dashLength = 2f;

    [Min(0.1f)]
    public float gapLength = 3f;

    [Min(0.01f)]
    public float lineWidth = 0.12f;

    [Header("Движение")]
    [Min(0f)]
    public float speed = 15f;

    private MeshFilter meshFilter;
    private Mesh generatedMesh;

    private readonly List<Vector3> vertices = new List<Vector3>(512);
    private readonly List<int> triangles = new List<int>(768);

    private float scrollOffset;

    private float CycleLength => dashLength + gapLength;
    private float HalfLength => roadLength * 0.5f;

    private void OnEnable()
    {
        BuildMesh();
    }

    private void Awake()
    {
        BuildMesh();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!Application.isPlaying)
            BuildMesh();
    }
#endif

    private void Update()
    {
        if (!Application.isPlaying)
            return;

        if (ChaseManager.Instance != null &&
            ChaseManager.Instance.IsGameOver())
            return;

        if (CycleLength <= 0.01f)
            return;

        // Двигаем рисунок разметки внутрь существующей длины дороги.
        // Из-за поворота объекта X=-90 увеличение локального Y
        // соответствует движению по мировому Z в сторону игрока.
        scrollOffset += speed * Time.deltaTime;

        while (scrollOffset >= CycleLength)
            scrollOffset -= CycleLength;

        UpdateMeshPositions();
    }

    private void BuildMesh()
    {
        if (meshFilter == null)
            meshFilter = GetComponent<MeshFilter>();

        if (meshFilter == null)
            return;

        if (generatedMesh != null)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                DestroyImmediate(generatedMesh);
            else
                Destroy(generatedMesh);
#else
            Destroy(generatedMesh);
#endif
        }

        generatedMesh = new Mesh
        {
            name = "RoadDashedLineMesh"
        };

        generatedMesh.MarkDynamic();

        meshFilter.sharedMesh = generatedMesh;

        scrollOffset = 0f;

        UpdateMeshPositions();
    }

    private void UpdateMeshPositions()
    {
        if (generatedMesh == null)
            return;

        vertices.Clear();
        triangles.Clear();

        if (roadLength <= 0f || CycleLength <= 0f)
            return;

        int dashCount = Mathf.CeilToInt(roadLength / CycleLength);

        float halfWidth = lineWidth * 0.5f;

        for (int i = 0; i < dashCount; i++)
        {
            float start = -HalfLength + i * CycleLength + scrollOffset;

            // Переносим штрих обратно в диапазон [-HalfLength, HalfLength].
            while (start >= HalfLength)
                start -= roadLength;

            while (start < -HalfLength)
                start += roadLength;

            float end = start + dashLength;

            // Обычный штрих, полностью внутри дороги.
            if (end <= HalfLength)
            {
                AddQuad(start, end, halfWidth);
            }
            else
            {
                // Штрих пересёк конец дороги.
                // Разрезаем его на две части:
                //
                // [start ----- конец]
                // [начало ----- end]
                //
                // Поэтому никакого исчезновения на границе нет.

                AddQuad(start, HalfLength, halfWidth);

                float wrappedEnd = end - roadLength;

                if (wrappedEnd > -HalfLength)
                    AddQuad(-HalfLength, wrappedEnd, halfWidth);
            }
        }

        generatedMesh.Clear();
        generatedMesh.SetVertices(vertices);
        generatedMesh.SetTriangles(triangles, 0);
        generatedMesh.RecalculateBounds();
        generatedMesh.RecalculateNormals();
    }

    private void AddQuad(float start, float end, float halfWidth)
    {
        if (end <= start)
            return;

        int index = vertices.Count;

        vertices.Add(new Vector3(-halfWidth, start, 0f));
        vertices.Add(new Vector3(halfWidth, start, 0f));
        vertices.Add(new Vector3(halfWidth, end, 0f));
        vertices.Add(new Vector3(-halfWidth, end, 0f));

        triangles.Add(index + 0);
        triangles.Add(index + 1);
        triangles.Add(index + 2);

        triangles.Add(index + 0);
        triangles.Add(index + 2);
        triangles.Add(index + 3);
    }
}