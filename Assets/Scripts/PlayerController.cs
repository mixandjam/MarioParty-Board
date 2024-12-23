using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Splines;

[RequireComponent(typeof(SplineKnotAnimate))]
public class PlayerController : MonoBehaviour
{
    private SplineKnotAnimate splineKnotAnimator;
    private SplineKnotInstantiate splineKnotData;
    [SerializeField] private int roll = 0;

    [Header("Parameters")]
    [SerializeField] private float fadeRollDelay = .5f;

    [Header("Events")]
    [HideInInspector] public UnityEvent OnRollStart;
    [HideInInspector] public UnityEvent<int, float> OnRollEnd;
    [HideInInspector] public UnityEvent<bool> OnMovementStart;
    [HideInInspector] public UnityEvent<int> OnMovementUpdate;

    [Header("States")]
    public bool isRolling;

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
        //Debug.Log($"Landed: S{data.knotIndex.Spline}K{data.knotIndex.Knot}");

        OnMovementStart.Invoke(false);
    }

    private void OnKnotEnter(SplineKnotIndex index)
    {
        SplineKnotData data = splineKnotData.splineDatas[index.Spline].knots[index.Knot];
        //Debug.Log($"Entered: S{data.knotIndex.Spline}K{data.knotIndex.Knot}");

        OnMovementUpdate.Invoke(splineKnotAnimator.Step);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (splineKnotAnimator.isMoving)
                return;

            if (splineKnotAnimator.inJunction)
            {
                splineKnotAnimator.inJunction = false;
            }
            else
            {
                if (isRolling)
                {
                    StartCoroutine(RollSequence());
                }
                else
                    PrepareToRoll();
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

    private void PrepareToRoll()
    {
        //Set rolling state
        isRolling = true;

        //Set unique camera

        //Rotate towards camera

        //Show spinning block;
        OnRollStart.Invoke();
    }

    IEnumerator RollSequence()
    {
        roll = Random.Range(1, 11);
        OnRollEnd.Invoke(roll, fadeRollDelay); ;
        OnMovementStart.Invoke(true);
        OnMovementUpdate.Invoke(roll);
        yield return new WaitForSeconds(fadeRollDelay);
        yield return new WaitForSeconds(.2f);
        isRolling = false;
        splineKnotAnimator.Animate(roll);
    }
}
