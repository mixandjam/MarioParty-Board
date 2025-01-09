using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [SerializeField] private int coins;
    public int Coins => coins;
    [SerializeField] private int stars;

    public void AddCoins(int amount)
    {
        coins += amount;
        coins = Mathf.Clamp(coins, 0, 999);
    }
}
