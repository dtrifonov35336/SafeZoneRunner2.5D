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

    [Header("Состояния смерти")]
    public string survivorDeathState = "Survivor_Death";
    public string militaryDeathState = "Military_Death";
    public string medicDeathState = "Medic_Death";
    public string firefighterDeathState = "Firefighter_Death";
    public string mechanicDeathState = "Mechanic_Death";
    public string scoutDeathState = "Scout_Death";

    [Header("Animator Parameters")]
    public string deathTriggerParameter = "Die";
    public string deathTypeParameter = "DeathType";

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

    private string[] DeathStateNames =>
        new string[]
        {
            survivorDeathState,
            militaryDeathState,
            medicDeathState,
            firefighterDeathState,
            mechanicDeathState,
            scoutDeathState
        };

    private void Awake()
    {
        animator = GetComponent<Animator>();

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
            ProfileManager.GetSelectedCharacterId();

        int index =
            GetCharacterIndex(charId);

        string stateName =
            RunStateNames[index];

        PlayState(
            stateName,
            true
        );

        animator.speed = 1f;

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
            ProfileManager.GetSelectedCharacterId();

        int index =
            GetCharacterIndex(charId);

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
                    $"[PlayerVisual] Не найдено состояние Animator: {stateName}"
                );
            }

            return;
        }

        animator.speed = 1f;

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

        animator.speed = 1f;

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

        StopAllCoroutines();

        animator.speed = 1f;

        string charId =
            ProfileManager.GetSelectedCharacterId();

        int index =
            GetCharacterIndex(charId);

        string deathState =
            DeathStateNames[index];

        // Указываем Animator, какой именно персонаж умер.
        if (
            HasParameter(
                deathTypeParameter,
                AnimatorControllerParameterType.Int
            )
        )
        {
            animator.SetInteger(
                deathTypeParameter,
                index
            );
        }

        // На всякий случай сбрасываем старый trigger.
        if (
            HasParameter(
                deathTriggerParameter,
                AnimatorControllerParameterType.Trigger
            )
        )
        {
            animator.ResetTrigger(
                deathTriggerParameter
            );

            animator.SetTrigger(
                deathTriggerParameter
            );
        }

        // Если состояние смерти существует,
        // сразу запускаем именно его.
        //
        // Это дополнительно защищает от ситуации,
        // когда переходы Animator настроены неправильно.
        if (
            !PlayState(
                deathState,
                true
            )
        )
        {
            if (debugLog)
            {
                Debug.LogWarning(
                    $"[PlayerVisual] Не найдено состояние смерти: {deathState}"
                );
            }
        }

        if (debugLog)
        {
            Debug.Log(
                $"[PlayerVisual] Смерть → {charId} → {deathState} → DeathType {index}"
            );
        }

        StartCoroutine(
            FreezeAfterDeath()
        );
    }

    private System.Collections.IEnumerator
        FreezeAfterDeath()
    {
        // Ждём окончания death-клипа.
        // Значение можно изменить после проверки
        // реальной длительности твоих клипов.
        yield return new WaitForSeconds(
            1.0f
        );

        if (animator != null)
        {
            animator.speed = 0f;
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

        animator.speed = 1f;

        if (
            HasParameter(
                deathTriggerParameter,
                AnimatorControllerParameterType.Trigger
            )
        )
        {
            animator.ResetTrigger(
                deathTriggerParameter
            );
        }

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
    // ANIMATOR PARAMETER CHECK
    // =========================================================

    private bool HasParameter(
        string parameterName,
        AnimatorControllerParameterType type
    )
    {
        if (
            animator == null ||
            string.IsNullOrEmpty(
                parameterName
            )
        )
        {
            return false;
        }

        AnimatorControllerParameter[] parameters =
            animator.parameters;

        for (
            int i = 0;
            i < parameters.Length;
            i++
        )
        {
            if (
                parameters[i].name ==
                    parameterName &&
                parameters[i].type ==
                    type
            )
            {
                return true;
            }
        }

        return false;
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
                CharacterIds[i] == id
            )
            {
                return i;
            }
        }

        return 0;
    }
}