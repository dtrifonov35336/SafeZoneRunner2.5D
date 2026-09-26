using UnityEngine;

public static class RunnerMovingObjectBlocker3D
{
    public static float ResolveZ(
        GameObject owner,
        float currentZ,
        float desiredZ,
        float x,
        float y,
        float gap)
    {
        if (owner == null || desiredZ >= currentZ)
            return desiredZ;

        Collider ownCollider =
            owner.GetComponent<Collider>();

        if (ownCollider == null)
            ownCollider =
                owner.GetComponentInChildren<Collider>();

        if (ownCollider == null)
            return desiredZ;

        Vector3 halfExtents =
            ownCollider.bounds.extents;

        Vector3 checkCenter =
            new Vector3(
                x,
                y,
                desiredZ
            );

        Collider[] hits =
            Physics.OverlapBox(
                checkCenter,
                halfExtents,
                Quaternion.identity,
                ~0,
                QueryTriggerInteraction.Collide
            );

        float resolvedZ = desiredZ;

        foreach (Collider hit in hits)
        {
            if (hit == null)
                continue;

            Transform hitTransform =
                hit.transform;

            if (hitTransform == owner.transform ||
                hitTransform.IsChildOf(owner.transform))
            {
                continue;
            }

            bool isObstacle =
                hit.GetComponentInParent<
                    ObstacleMover3D>() != null;

            bool isPickup =
                hit.GetComponentInParent<
                    PickupMover3D>() != null;

            bool isRescued =
                hit.GetComponentInParent<
                    RescuedPerson>() != null;

            if (!isObstacle &&
                !isPickup &&
                !isRescued)
            {
                continue;
            }

            Bounds otherBounds =
                hit.bounds;

            if (otherBounds.max.z <= desiredZ)
                continue;

            float stopZ =
                otherBounds.max.z +
                halfExtents.z +
                gap;

            if (stopZ >= currentZ)
                continue;

            if (stopZ > resolvedZ)
                resolvedZ = stopZ;
        }

        return resolvedZ;
    }
}