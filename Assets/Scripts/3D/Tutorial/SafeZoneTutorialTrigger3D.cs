using UnityEngine;

public class SafeZoneTutorialTrigger3D : MonoBehaviour
{
    [Header("Этап")]
    public SafeZoneTutorialManager3D.TutorialStage stage;

    [Header("Настройки")]
    public bool destroyAfterEnter = false;
    public bool requirePlayer = true;

    private bool triggered;

    private void OnTriggerEnter(
        Collider other
    )
    {
        if (triggered)
        {
            return;
        }

        if (requirePlayer)
        {
            PlayerMovement3D player =
                other.GetComponentInParent<
                    PlayerMovement3D
                >();

            if (player == null)
            {
                return;
            }
        }

        SafeZoneTutorialManager3D manager =
            SafeZoneTutorialManager3D.Instance;

        if (manager == null)
        {
            return;
        }

        if (!manager.IsRunning)
        {
            return;
        }

        if (!manager.IsStage(stage))
        {
            return;
        }

        triggered = true;

        if (destroyAfterEnter)
        {
            Destroy(gameObject);
        }
    }

    public void ResetTrigger()
    {
        triggered = false;
    }
}