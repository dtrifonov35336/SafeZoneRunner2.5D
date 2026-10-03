using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "CharacterAvailabilityConfig",
    menuName = "Safe Zone Runner/Character Availability"
)]
public class CharacterAvailabilityConfig : ScriptableObject
{
    [System.Serializable]
    public class CharacterAvailability
    {
        [Header("Персонаж")]
        public string id;

        [Header("Доступен для покупки")]
        public bool availableForPurchase = false;
    }

    [Header("Доступность персонажей")]
    public List<CharacterAvailability> characters =
        new List<CharacterAvailability>();

    public bool IsAvailable(string characterId)
    {
        if (string.IsNullOrEmpty(characterId))
            return false;

        if (
            characterId.ToLowerInvariant() ==
            "survivor"
        )
        {
            return true;
        }

        for (
            int i = 0;
            i < characters.Count;
            i++
        )
        {
            if (
                characters[i] != null &&
                characters[i].id == characterId
            )
            {
                return characters[i].availableForPurchase;
            }
        }

        return false;
    }
}