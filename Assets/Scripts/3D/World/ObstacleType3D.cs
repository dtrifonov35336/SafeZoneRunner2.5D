using UnityEngine;

public enum ObstacleType
{
    Normal,
    Pit,
    Slide,
    DoubleJump
}

public class ObstacleType3D : MonoBehaviour
{
    [Header("Тип препятствия")]
    public ObstacleType type = ObstacleType.Normal;
}