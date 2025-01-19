using UnityEngine;
using UnityEngine.Events;

public class PlayerStats : MonoBehaviour
{
    [SerializeField] private int coins;
    public int Coins => coins;
    [SerializeField] private int stars;
    public int Stars => stars;
    public int coinsBeforeChange;

    [HideInInspector] public UnityEvent OnInitialize;
    [HideInInspector] public UnityEvent<int> OnAnimation;

    private void Start()
    {
        OnInitialize.Invoke();
        coinsBeforeChange = coins;
    }

    public void AddCoins(int amount)
    {
        coinsBeforeChange = coins;
        coins += amount;
        coins = Mathf.Clamp(coins, 0, 999);
    }

    public void AddStars(int amount)
    {
        stars += amount;
        stars = Mathf.Clamp(stars, 0, 999);
    }


    public void CoinAnimation(int value)
    {
        coinsBeforeChange += value;
        OnAnimation.Invoke(coinsBeforeChange);
    }

    public void UpdateStats()
    {
        OnInitialize.Invoke();
        coinsBeforeChange = coins;
    }
}
