using UnityEngine;
using UnityEngine.UI;

public class TaskCheckmarkGraphic3D : MaskableGraphic
{
    [Range(2f, 12f)]
    public float thickness = 5f;

    protected override void OnPopulateMesh(
        VertexHelper vh)
    {
        vh.Clear();

        Rect rect =
            rectTransform.rect;

        Vector2 a =
            new Vector2(
                rect.xMin +
                rect.width * 0.16f,
                rect.yMin +
                rect.height * 0.48f
            );

        Vector2 b =
            new Vector2(
                rect.xMin +
                rect.width * 0.42f,
                rect.yMin +
                rect.height * 0.24f
            );

        Vector2 c =
            new Vector2(
                rect.xMin +
                rect.width * 0.84f,
                rect.yMin +
                rect.height * 0.76f
            );

        AddLine(
            vh,
            a,
            b
        );

        AddLine(
            vh,
            b,
            c
        );
    }

    private void AddLine(
        VertexHelper vh,
        Vector2 start,
        Vector2 end)
    {
        Vector2 direction =
            (end - start).normalized;

        Vector2 normal =
            new Vector2(
                -direction.y,
                direction.x
            ) *
            (thickness * 0.5f);

        UIVertex v0 =
            UIVertex.simpleVert;

        UIVertex v1 =
            UIVertex.simpleVert;

        UIVertex v2 =
            UIVertex.simpleVert;

        UIVertex v3 =
            UIVertex.simpleVert;

        Color vertexColor =
            color;

        v0.color =
            vertexColor;

        v1.color =
            vertexColor;

        v2.color =
            vertexColor;

        v3.color =
            vertexColor;

        v0.position =
            start + normal;

        v1.position =
            end + normal;

        v2.position =
            end - normal;

        v3.position =
            start - normal;

        vh.AddUIVertexQuad(
            new[]
            {
                v0,
                v1,
                v2,
                v3
            }
        );
    }
}