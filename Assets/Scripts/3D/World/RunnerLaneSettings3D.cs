using UnityEngine;

public static class RunnerLaneSettings3D
{
    // Центральная линия дороги = 0.
    // Полосы находятся симметрично относительно центра.
    public const float LaneOffset = 1.05f;

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