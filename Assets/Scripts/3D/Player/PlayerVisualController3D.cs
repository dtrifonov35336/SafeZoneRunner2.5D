using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerVisualController : MonoBehaviour
{
    [Header("Отладка")]
    public bool debugLog = true;

    [Header("Состояния бега")]
    public string survivorRunState = "Survivor_Run";
    public string militaryRunState = "Military_Run";
    public string medicRunState = "Medic_Run";
    public string firefighterRunState = "Firefighter_Run";
    public string mechanicRunState = "Mechanic_Run";
    public string scoutRunState = "Scout_Run";

    [Header("Состояния скольжения")]
    public string survivorSlideState = "Survivor_Slide";
    public string militarySlideState = "Military_Slide";
    public string medicSlideState = "Medic_Slide";
    public string firefighterSlideState = "Firefighter_Slide";
    public string mechanicSlideState = "Mechanic_Slide";
    public string scoutSlideState = "Scout_Slide";

    private Animator animator;

    private static readonly string[] CharacterIds =
    {
        "survivor",
        "military",
        "medic",
        "firefighter",
        "mechanic",
        "scout"
    };

    private string[] RunStateNames =>
        new string[]
        {
            survivorRunState,
            militaryRunState,
            medicRunState,
            firefighterRunState,
            mechanicRunState,
            scoutRunState
        };

    private string[] SlideStateNames =>
        new string[]
        {
            survivorSlideState,
            militarySlideState,
            medicSlideState,
            firefighterSlideState,
            mechanicSlideState,
            scoutSlideState
        };

    private void Awake()
    {
        animator =
            GetComponent<Animator>();

        ApplySelectedCharacter();
    }

    // =========================================================
    // CHARACTER
    // =========================================================

    private void ApplySelectedCharacter()
    {
        if (animator == null)
            return;

        string charId =
            ProfileManager
                .GetSelectedCharacterId();

        int index =
            GetCharacterIndex(
                charId
            );

        string stateName =
            RunStateNames[index];

        PlayState(
            stateName,
            true
        );

        animator.speed =
            1f;

        if (debugLog)
        {
            Debug.Log(
                $"[PlayerVisual] Персонаж: {charId} → {stateName}"
            );
        }
    }

    // =========================================================
    // SLIDE
    // =========================================================

    public void StartSlideAnimation()
    {
        if (animator == null)
            return;

        string charId =
            ProfileManager
                .GetSelectedCharacterId();

        int index =
            GetCharacterIndex(
                charId
            );

        string stateName =
            SlideStateNames[index];

        if (!PlayState(
                stateName,
                true
            ))
        {
            if (debugLog)
            {
                Debug.LogWarning(
                    $"[PlayerVisual] " +
                    $"Не найдено состояние Animator: {stateName}"
                );
            }

            return;
        }

        animator.speed =
            1f;

        if (debugLog)
        {
            Debug.Log(
                $"[PlayerVisual] Слайд → {stateName}"
            );
        }
    }

    public void EndSlideAnimation()
    {
        if (animator == null)
            return;

        animator.speed =
            1f;

        ApplySelectedCharacter();

        if (debugLog)
        {
            Debug.Log(
                "[PlayerVisual] Слайд завершён → бег"
            );
        }
    }

    // =========================================================
    // DEATH
    // =========================================================

    public void TriggerDeath()
    {
        if (animator == null)
            return;

        animator.speed =
            1f;

        animator.SetTrigger(
            "Die"
        );

        if (debugLog)
        {
            Debug.Log(
                "[PlayerVisual] Trigger Die"
            );
        }

        StopAllCoroutines();

        StartCoroutine(
            FreezeAfterDeath()
        );
    }

    private System.Collections.IEnumerator
        FreezeAfterDeath()
    {
        yield return
            new WaitForSeconds(
                1.0f
            );

        if (animator != null)
        {
            animator.speed =
                0f;
        }

        if (debugLog)
        {
            Debug.Log(
                "[PlayerVisual] Анимация смерти заморожена"
            );
        }
    }

    // =========================================================
    // REVIVE
    // =========================================================

    public void ReviveAnimation()
    {
        if (animator == null)
            return;

        StopAllCoroutines();

        animator.speed =
            1f;

        animator.ResetTrigger(
            "Die"
        );

        ApplySelectedCharacter();

        if (debugLog)
        {
            Debug.Log(
                "[PlayerVisual] Анимация возрождена"
            );
        }
    }

    // =========================================================
    // REFRESH
    // =========================================================

    public void RefreshCharacter()
    {
        ApplySelectedCharacter();
    }

    // =========================================================
    // PLAY STATE
    // =========================================================

    private bool PlayState(
        string stateName,
        bool restart
    )
    {
        if (
            animator == null ||
            string.IsNullOrEmpty(
                stateName
            )
        )
        {
            return false;
        }

        int hash =
            Animator.StringToHash(
                stateName
            );

        if (
            !animator.HasState(
                0,
                hash
            )
        )
        {
            return false;
        }

        animator.Play(
            hash,
            0,
            restart
                ? 0f
                : float.NegativeInfinity
        );

        return true;
    }

    // =========================================================
    // INDEX
    // =========================================================

    private int GetCharacterIndex(
        string id
    )
    {
        for (
            int i = 0;
            i < CharacterIds.Length;
            i++
        )
        {
            if (
                CharacterIds[i] ==
                id
            )
            {
                return i;
            }
        }

        return 0;
    }
}