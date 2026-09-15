using UnityEngine;

public enum PickupType { Coin, Heart, Diamond }

public class Pickup : MonoBehaviour
{
    public PickupType type = PickupType.Coin;
    public int amount = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (HUDManager.Instance == null) return;

        switch (type)
        {
            case PickupType.Coin:
                HUDManager.Instance.AddCoins(amount);
                break;
            case PickupType.Heart:
                HUDManager.Instance.AddHealth(amount);
                break;
            case PickupType.Diamond:
                HUDManager.Instance.AddDiamonds(amount);
                break;
        }

        Destroy(gameObject);
    }
}