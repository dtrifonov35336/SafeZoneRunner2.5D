using UnityEngine;

public static class RunnerLaneSettings3D
{
    // Центры двух игровых полос.
    // Расстояние между ними = 2.7.
    public const float LaneOffset = 1.35f;

    public static float GetCenterX()
    {
        GameObject centerObject =
            GameObject.Find(
                "RoadCenterMarking"
            );

        if (centerObject != null)
        {
            return centerObject.transform.position.x;
        }

        RoadDashedLine line =
            Object.FindFirstObjectByType<
                RoadDashedLine
            >();

        if (line != null)
        {
            return line.transform.position.x;
        }

        return 0f;
    }

    public static float GetLaneX(
        int index
    )
    {
        float centerX =
            GetCenterX();

        if (index <= 0)
        {
            return centerX -
                   LaneOffset;
        }

        return centerX +
               LaneOffset;
    }

    public static float[] GetLanePositions()
    {
        float centerX =
            GetCenterX();

        return new float[]
        {
            centerX - LaneOffset,
            centerX + LaneOffset
        };
    }
}