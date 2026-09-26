using UnityEngine;

public enum Pickup3DType
{
    Coin,
    Heart,
    Diamond
}

public class Pickup3D : MonoBehaviour
{
    public Pickup3DType type = Pickup3DType.Coin;
    public int amount = 1;

    private bool collected = false;

    private void OnTriggerEnter(Collider other)
    {
        if (collected)
            return;

        if (!other.CompareTag("Player"))
            return;

        if (HUDManager.Instance == null)
            return;

        collected = true;

        switch (type)
        {
            case Pickup3DType.Coin:
                // Одна монета = одно начисление.
                // Бонусный множитель здесь специально не применяется.
                HUDManager.Instance.AddCoins(amount);
                break;

            case Pickup3DType.Heart:
                HUDManager.Instance.AddHealth(amount);
                break;

            case Pickup3DType.Diamond:
                HUDManager.Instance.AddDiamonds(amount);
                break;
        }

        Destroy(gameObject);
    }
}