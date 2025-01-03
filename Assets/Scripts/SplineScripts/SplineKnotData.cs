using UnityEngine;
using UnityEngine.Splines;

public class SplineKnotData : MonoBehaviour
{
    public SplineKnotIndex knotIndex;

    public int coinGain = 3;

    private void OnValidate()
    {
        //gameObject.hideFlags = HideFlags.NotEditable;
    }

}
