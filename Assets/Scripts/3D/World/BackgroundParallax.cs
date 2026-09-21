using UnityEngine;

public class BackgroundParallax : MonoBehaviour
{
    [Range(0f, 1f)]
    public float parallaxFactor = 0.1f;

    private void Update()
    {
        if (ChaseManager.Instance != null &&
            ChaseManager.Instance.IsGameOver())
            return;

        // Дальний фон не движется по Z.
        // Perspective больше не увеличивает его
        // по мере прохождения забега.
    }
}