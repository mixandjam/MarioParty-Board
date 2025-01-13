using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Splines;

public class SplineKnotData : MonoBehaviour
{
    public SplineKnotIndex knotIndex;

    [HideInInspector] public UnityEvent<int> OnLand;

    public int coinGain = 3;

    [SerializeField] private bool pauseMovement = false;
    [SerializeField] private bool skipStepCount = false;

    private void OnValidate()
    {
        //gameObject.hideFlags = HideFlags.NotEditable;
    }

    public void EnterKnot(SplineKnotAnimate splineKnotAnimator)
    {
        splineKnotAnimator.Paused = pauseMovement;
        //splineKnotAnimator.SkipStepCount = skipStepCount;
    }

    public void Land(PlayerStats playerStats)
    {
        playerStats.AddCoins(coinGain);
        OnLand.Invoke(coinGain);

    }

}
