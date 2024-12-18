using UnityEngine;
using UnityEngine.Splines;

public class SplineKnotData : MonoBehaviour
{
    public SplineKnotIndex knotIndex;

    private void OnValidate()
    {
        gameObject.hideFlags = HideFlags.NotEditable;
    }

}
