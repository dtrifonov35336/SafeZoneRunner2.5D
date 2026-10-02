using UnityEngine;

public static class RunnerLaneSettings3D
{
    // Расстояние от центра дороги до каждой полосы.
    // Было 1.05 — уменьшаем, чтобы игрок и монеты
    // не уходили слишком далеко влево/вправо.
    public const float LaneOffset = 0.95f;

    public static float GetCenterX()
    {
        GameObject centerObject =
            GameObject.Find("RoadCenterMarking");

        if (centerObject != null)
        {
            return centerObject.transform.position.x;
        }

        RoadDashedLine line =
            Object.FindFirstObjectByType<RoadDashedLine>();

        if (line != null)
        {
            return line.transform.position.x;
        }

        return 0f;
    }

    public static float GetLaneX(int index)
    {
        float centerX =
            GetCenterX();

        if (index <= 0)
        {
            return centerX - LaneOffset;
        }

        return centerX + LaneOffset;
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