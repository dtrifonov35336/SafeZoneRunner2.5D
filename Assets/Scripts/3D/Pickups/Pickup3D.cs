using UnityEngine;

public enum Pickup3DType
{
    Coin,
    Heart,
    Diamond
}

public class Pickup3D : MonoBehaviour
{
    public Pickup3DType type =
        Pickup3DType.Coin;

    public int amount = 1;

    private bool collected = false;

    private void OnTriggerEnter(
        Collider other)
    {
        if (collected)
            return;

        if (!other.CompareTag("Player"))
            return;

        if (HUDManager.Instance == null)
            return;

        collected = true;

        string charId =
            ProfileManager.GetSelectedCharacterId();

        switch (type)
        {
            case Pickup3DType.Coin:
                {
                    float multiplier =
                        BonusCalculator
                            .GetCoinPickupMultiplier(
                                charId
                            );

                    int finalAmount =
                        Mathf.Max(
                            1,
                            Mathf.RoundToInt(
                                amount *
                                multiplier
                            )
                        );

                    HUDManager.Instance.AddCoins(
                        finalAmount
                    );

                    break;
                }

            case Pickup3DType.Heart:
                {
                    float multiplier =
                        BonusCalculator
                            .GetHeartHealMultiplier(
                                charId
                            );

                    float finalHeal =
                        amount *
                        multiplier;

                    HUDManager.Instance.AddHealth(
                        finalHeal
                    );

                    break;
                }

            case Pickup3DType.Diamond:
                {
                    HUDManager.Instance.AddDiamonds(
                        amount
                    );

                    break;
                }
        }

        Destroy(gameObject);
    }
}