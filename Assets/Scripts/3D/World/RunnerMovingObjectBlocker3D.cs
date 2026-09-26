using UnityEngine;

public static class RunnerMovingObjectBlocker3D
{
    // =========================================================
    // ОСНОВНОЙ МЕТОД
    // =========================================================

    public static float ResolveZ(
        GameObject owner,
        float currentZ,
        float desiredZ,
        float x,
        float y,
        float gap)
    {
        if (owner == null)
            return desiredZ;

        if (desiredZ >= currentZ)
            return desiredZ;

        Collider ownCollider =
            owner.GetComponent<Collider>();

        if (ownCollider == null)
        {
            ownCollider =
                owner.GetComponentInChildren<Collider>();
        }

        if (ownCollider == null)
            return desiredZ;

        Bounds ownBounds =
            ownCollider.bounds;

        float ownHalfX =
            ownBounds.extents.x;

        float ownHalfZ =
            ownBounds.extents.z;

        float ownMinX =
            x - ownHalfX;

        float ownMaxX =
            x + ownHalfX;

        float resolvedZ =
            desiredZ;

        // -----------------------------------------------------
        // ПРЕПЯТСТВИЯ
        // -----------------------------------------------------

        ObstacleMover3D[] obstacles =
            Object.FindObjectsByType<ObstacleMover3D>(
                FindObjectsSortMode.None
            );

        foreach (ObstacleMover3D obstacle in obstacles)
        {
            if (obstacle == null)
                continue;

            if (obstacle.gameObject == owner)
                continue;

            Collider obstacleCollider =
                obstacle.GetComponent<Collider>();

            if (obstacleCollider == null)
            {
                obstacleCollider =
                    obstacle.GetComponentInChildren<Collider>();
            }

            if (obstacleCollider == null)
                continue;

            Bounds otherBounds =
                obstacleCollider.bounds;

            // Проверяем пересечение по X.
            if (ownMaxX < otherBounds.min.x ||
                ownMinX > otherBounds.max.x)
            {
                continue;
            }

            float stopZ =
                otherBounds.max.z +
                ownHalfZ +
                gap;

            // Препятствие должно находиться впереди.
            if (stopZ >= currentZ)
                continue;

            // Текущий кадр пересекает границу препятствия.
            if (desiredZ <= stopZ &&
                currentZ > stopZ)
            {
                if (stopZ > resolvedZ)
                    resolvedZ = stopZ;
            }
        }

        // -----------------------------------------------------
        // ДРУГИЕ PICKUP
        // -----------------------------------------------------

        PickupMover3D[] pickups =
            Object.FindObjectsByType<PickupMover3D>(
                FindObjectsSortMode.None
            );

        foreach (PickupMover3D pickup in pickups)
        {
            if (pickup == null)
                continue;

            if (pickup.gameObject == owner)
                continue;

            Pickup3D pickupData =
                pickup.GetComponent<Pickup3D>();

            // Монеты не блокируют движение.
            if (pickupData != null &&
                pickupData.type == Pickup3DType.Coin)
            {
                continue;
            }

            Collider pickupCollider =
                pickup.GetComponent<Collider>();

            if (pickupCollider == null)
            {
                pickupCollider =
                    pickup.GetComponentInChildren<Collider>();
            }

            if (pickupCollider == null)
                continue;

            Bounds otherBounds =
                pickupCollider.bounds;

            if (ownMaxX < otherBounds.min.x ||
                ownMinX > otherBounds.max.x)
            {
                continue;
            }

            float stopZ =
                otherBounds.max.z +
                ownHalfZ +
                gap;

            if (stopZ >= currentZ)
                continue;

            if (desiredZ <= stopZ &&
                currentZ > stopZ)
            {
                if (stopZ > resolvedZ)
                    resolvedZ = stopZ;
            }
        }

        // -----------------------------------------------------
        // СПАСАЕМЫЕ
        // -----------------------------------------------------

        RescuedPerson[] rescued =
            Object.FindObjectsByType<RescuedPerson>(
                FindObjectsSortMode.None
            );

        foreach (RescuedPerson person in rescued)
        {
            if (person == null)
                continue;

            if (person.gameObject == owner)
                continue;

            Collider personCollider =
                person.GetComponent<Collider>();

            if (personCollider == null)
            {
                personCollider =
                    person.GetComponentInChildren<Collider>();
            }

            if (personCollider == null)
                continue;

            Bounds otherBounds =
                personCollider.bounds;

            if (ownMaxX < otherBounds.min.x ||
                ownMinX > otherBounds.max.x)
            {
                continue;
            }

            float stopZ =
                otherBounds.max.z +
                ownHalfZ +
                gap;

            if (stopZ >= currentZ)
                continue;

            if (desiredZ <= stopZ &&
                currentZ > stopZ)
            {
                if (stopZ > resolvedZ)
                    resolvedZ = stopZ;
            }
        }

        return resolvedZ;
    }

    // =========================================================
    // СОВМЕСТИМОСТЬ С PICKUPMOVER3D
    // =========================================================
    //
    // Позволяет PickupMover3D использовать вызов:
    //
    // ResolveZ(
    //     gameObject,
    //     currentZ,
    //     desiredZ,
    //     laneX
    // );
    //
    // =========================================================

    public static float ResolveZ(
        GameObject owner,
        float currentZ,
        float desiredZ,
        float x)
    {
        float y =
            owner != null
                ? owner.transform.position.y
                : 0f;

        return ResolveZ(
            owner,
            currentZ,
            desiredZ,
            x,
            y,
            0.03f
        );
    }
}