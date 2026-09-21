using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class RoadDashedLine : MonoBehaviour
{
    [Header("Длина разметки")]
    [Min(1f)]
    public float roadLength = 300f;

    [Header("Размер одного штриха")]
    [Min(0.1f)]
    public float dashLength = 2.4f;

    [Header("Расстояние между штрихами")]
    [Min(0.1f)]
    public float gapLength = 3.0f;

    [Header("Ширина линии")]
    [Min(0.01f)]
    public float lineWidth = 0.12f;

    private MeshFilter meshFilter;
    private Mesh generatedMesh;

    private void OnEnable()
    {
        Build();
    }

    private void Awake()
    {
        Build();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!Application.isPlaying)
            Build();
    }
#endif

    private void Build()
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

        float halfLength = roadLength * 0.5f;

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        float currentY = -halfLength;

        while (currentY < halfLength)
        {
            float dashStart = currentY;
            float dashEnd = Mathf.Min(currentY + dashLength, halfLength);

            if (dashEnd > dashStart)
            {
                int index = vertices.Count;

                float halfWidth = lineWidth * 0.5f;

                // Quad в локальной XY-плоскости.
                vertices.Add(new Vector3(-halfWidth, dashStart, 0f));
                vertices.Add(new Vector3(halfWidth, dashStart, 0f));
                vertices.Add(new Vector3(halfWidth, dashEnd, 0f));
                vertices.Add(new Vector3(-halfWidth, dashEnd, 0f));

                triangles.Add(index + 0);
                triangles.Add(index + 1);
                triangles.Add(index + 2);

                triangles.Add(index + 0);
                triangles.Add(index + 2);
                triangles.Add(index + 3);
            }

            currentY += dashLength + gapLength;
        }

        generatedMesh.SetVertices(vertices);
        generatedMesh.SetTriangles(triangles, 0);
        generatedMesh.RecalculateBounds();
        generatedMesh.RecalculateNormals();

        meshFilter.sharedMesh = generatedMesh;
    }
}