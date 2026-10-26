using UnityEngine;

public class SpawnHeightOffset3D : MonoBehaviour
{
    [Header("Настройка высоты")]
    [Tooltip("По умолчанию объект автоматически ставится на поверхность дороги. Включай только для объекта, который намеренно должен иметь заданную высоту.")]
    public bool useCustomHeight = false;

    [Tooltip("Абсолютная мировая высота Y. Используется только при включённом Use Custom Height.")]
    public float spawnY = 0f;

    [Tooltip("Небольшая дополнительная поправка после автоматического выравнивания. Обычно оставлять 0.")]
    public float groundOffset = 0f;
}
