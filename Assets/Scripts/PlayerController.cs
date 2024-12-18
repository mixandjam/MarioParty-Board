using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Splines;

[RequireComponent(typeof(SplineKnotAnimate))]
public class PlayerController : MonoBehaviour
{
    private SplineKnotAnimate splineKnotAnimator;
    private SplineKnotInstantiate splineKnotData;
    [SerializeField] private int roll = 0;
    private int currentRoll;

    [Header("Events")]
    [HideInInspector] public UnityEvent<bool> OnRollStart;
    [HideInInspector] public UnityEvent<int> OnRollUpdate;

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

        OnRollStart.Invoke(false);
    }

    private void OnKnotEnter(SplineKnotIndex index)
    {
        SplineKnotData data = splineKnotData.splineDatas[index.Spline].knots[index.Knot];
        Debug.Log($"Entered: S{data.knotIndex.Spline}K{data.knotIndex.Knot}");

        currentRoll = splineKnotAnimator.Step;

        OnRollUpdate.Invoke(currentRoll);
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
                currentRoll = roll;
                OnRollStart.Invoke(true);
                OnRollUpdate.Invoke(roll);
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
