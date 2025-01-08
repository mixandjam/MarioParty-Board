using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Splines;

public class SplineKnotData : MonoBehaviour
{
    public SplineKnotIndex knotIndex;

    [HideInInspector] public UnityEvent<int> OnLand;

    public int coinGain = 3;

    private void OnValidate()
    {
        //gameObject.hideFlags = HideFlags.NotEditable;
    }

    public void Land(PlayerStats playerStats)
    {
        playerStats.AddCoins(coinGain);
        OnLand.Invoke(coinGain);

    }

}
