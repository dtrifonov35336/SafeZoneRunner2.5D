using UnityEngine;

public enum Pickup3DType { Coin, Heart, Diamond }

public class Pickup3D : MonoBehaviour
{
    public Pickup3DType type = Pickup3DType.Coin;
    public int amount = 1;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (HUDManager.Instance == null) return;

        switch (type)
        {
            case Pickup3DType.Coin:
                HUDManager.Instance.AddCoinsWithBonus(amount);
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