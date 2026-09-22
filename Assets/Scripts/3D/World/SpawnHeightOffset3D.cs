using UnityEngine;

public class SpawnHeightOffset3D : MonoBehaviour
{
    [Header("Высота появления")]
    [Tooltip(
        "Абсолютная мировая высота Y. " +
        "Не прибавляется к Spawn Y спавнера."
    )]
    public float spawnY = 0f;
}