using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerVisualController : MonoBehaviour
{
    [Header("Отладка")]
    public bool debugLog = true;

    private Animator animator;

    private static readonly string[] CharacterIds = {
        "survivor", "military", "medic", "firefighter", "mechanic", "scout"
    };

    private static readonly string[] StateNames = {
        "Survivor_Run", "Military_Run", "Medic_Run",
        "Firefighter_Run", "Mechanic_Run", "Scout_Run"
    };

    private void Awake()
    {
        animator = GetComponent<Animator>();
        ApplySelectedCharacter();
    }

    private void ApplySelectedCharacter()
    {
        string charId = ProfileManager.GetSelectedCharacterId();
        int index = GetCharacterIndex(charId);

        string stateName = StateNames[index];
        animator.Play(stateName, 0, 0f);
        animator.speed = 1f;

        if (debugLog)
            Debug.Log($"[PlayerVisual] Персонаж: {charId} → стейт {stateName}");
    }

    public void TriggerDeath()
    {
        if (animator == null) return;

        animator.speed = 1f;
        animator.SetTrigger("Die");

        if (debugLog)
            Debug.Log("[PlayerVisual] Trigger Die");

        StopAllCoroutines();
        StartCoroutine(FreezeAfterDeath());
    }

    private System.Collections.IEnumerator FreezeAfterDeath()
    {
        yield return new WaitForSeconds(1.0f);

        if (animator != null)
            animator.speed = 0f;

        if (debugLog)
            Debug.Log("[PlayerVisual] Анимация смерти заморожена");
    }

    /// <summary>Возвращает визуал в состояние бега после revive.</summary>
    public void ReviveAnimation()
    {
        if (animator == null) return;

        StopAllCoroutines();
        animator.speed = 1f;
        animator.ResetTrigger("Die");
        ApplySelectedCharacter();

        if (debugLog)
            Debug.Log("[PlayerVisual] Анимация возрождена");
    }

    public void RefreshCharacter()
    {
        ApplySelectedCharacter();
    }

    private int GetCharacterIndex(string id)
    {
        for (int i = 0; i < CharacterIds.Length; i++)
        {
            if (CharacterIds[i] == id) return i;
        }
        return 0;
    }
}