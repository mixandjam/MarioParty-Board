using UnityEngine;
using UnityEngine.Splines;

[RequireComponent(typeof(SplineKnotAnimate))]
public class PlayerController : MonoBehaviour
{
    private SplineKnotAnimate splineKnotAnimator;
    private SplineKnotInstantiate splineKnotData;
    [SerializeField] private int roll = 0;

    void Start()
    {
        splineKnotAnimator = GetComponent<SplineKnotAnimate>();

        splineKnotAnimator.OnKnotEnter.AddListener(OnKnotEnter);
        splineKnotAnimator.OnKnotLand.AddListener(OnKnotLand);

        if (FindAnyObjectByType<SplineKnotInstantiate>() != null)
            splineKnotData = FindAnyObjectByType<SplineKnotInstantiate>();
    }

    private void OnKnotLand(SplineKnotIndex index)
    {
        SplineKnotData data = splineKnotData.splineDatas[index.Spline].knots[index.Knot];
        Debug.Log($"Landed: S{data.knotIndex.Spline}K{data.knotIndex.Knot}");
    }

    private void OnKnotEnter(SplineKnotIndex index)
    {
        SplineKnotData data = splineKnotData.splineDatas[index.Spline].knots[index.Knot];
        Debug.Log($"Entered: S{data.knotIndex.Spline}K{data.knotIndex.Knot}");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (splineKnotAnimator.inJunction)
            {
                splineKnotAnimator.inJunction = false;
            }
            else if (!splineKnotAnimator.isMoving)
            {
                roll = Random.Range(1, 10);
                splineKnotAnimator.Animate(roll);
            }
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            splineKnotAnimator.AddToJunctionIndex(1);
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            splineKnotAnimator.AddToJunctionIndex(-1);
        }
    }
}
