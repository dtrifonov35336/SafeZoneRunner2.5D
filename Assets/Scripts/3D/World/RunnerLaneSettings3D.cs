using UnityEngine;

public static class RunnerLaneSettings3D
{
    // =========================================================
    // ПОЛОСЫ
    // =========================================================

    public const float LaneOffset = 0.8f;

    // =========================================================
    // ЦЕНТР ДОРОГИ
    // =========================================================

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

    // =========================================================
    // ПОЛОСА
    // =========================================================

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

    // =========================================================
    // ВСЕ ПОЛОСЫ
    // =========================================================

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