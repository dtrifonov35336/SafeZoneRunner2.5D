using UnityEngine;

public class PickupMover3D : MonoBehaviour
{
    [Header("Траектория (Z)")]
    public float spawnZ = 60f;
    public float despawnZ = -3f;

    [Header("Движение")]
    public float speed = 15f;

    [Header("Полоса")]
    public float laneX = 0f;

    private float currentZ;
    private bool initialised = false;

    // =========================================================
    // НАЧАЛЬНОЕ СОСТОЯНИЕ
    // =========================================================

    public void ApplyInitialState()
    {
        currentZ = spawnZ;

        Vector3 p =
            transform.position;

        p.x = laneX;
        p.z = spawnZ;

        transform.position = p;

        initialised = true;
    }

    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        if (!initialised)
        {
            ApplyInitialState();
        }
    }

    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        currentZ -=
            speed *
            Time.deltaTime;

        float desiredZ =
            currentZ;

        // -----------------------------------------------------
        // МОНЕТЫ
        // -----------------------------------------------------
        // Монеты специально должны проходить через область
        // препятствия по заданной траектории.
        //
        // Поэтому НЕ применяем к ним
        // RunnerMovingObjectBlocker3D.
        // -----------------------------------------------------

        Pickup3D pickup =
            GetComponent<Pickup3D>();

        bool isCoin =
            pickup != null &&
            pickup.type ==
            Pickup3DType.Coin;

        if (!isCoin)
        {
            // Сердечки и другие движущиеся pickup'ы
            // не должны накладываться на другие объекты.
            desiredZ =
                RunnerMovingObjectBlocker3D.ResolveZ(
                    gameObject,
                    currentZ,
                    desiredZ,
                    laneX
                );
        }

        currentZ =
            desiredZ;

        Vector3 p =
            transform.position;

        p.x =
            laneX;

        p.z =
            currentZ;

        transform.position =
            p;

        // -----------------------------------------------------
        // УДАЛЕНИЕ
        // -----------------------------------------------------

        if (currentZ <= despawnZ)
        {
            Destroy(gameObject);
        }
    }
}