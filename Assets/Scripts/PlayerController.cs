using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Splines;

[RequireComponent(typeof(SplineKnotAnimate))]
public class PlayerController : MonoBehaviour
{
    private SplineKnotAnimate splineKnotAnimator;
    private SplineKnotInstantiate splineKnotData;
    [SerializeField] private int roll = 0;

    [Header("Parameters")]
    [SerializeField] private float jumpDelay = .5f;
    [SerializeField]
    private float resultDelay = .5f;
    [SerializeField] private float startMoveDelay = .5f;

    [Header("Events")]
    [HideInInspector] public UnityEvent OnRollStart;
    [HideInInspector] public UnityEvent OnRollJump;
    [HideInInspector] public UnityEvent<int> OnRollDisplay;
    [HideInInspector] public UnityEvent OnRollEnd;
    [HideInInspector] public UnityEvent OnRollCancel;
    [HideInInspector] public UnityEvent<bool> OnMovementStart;
    [HideInInspector] public UnityEvent<int> OnMovementUpdate;

    [Header("States")]
    public bool isRolling;
    public bool allowInput = true;

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

        StartCoroutine(DelayCoroutine());
        IEnumerator DelayCoroutine()
        {
            yield return new WaitForSeconds(.08f);
            OnMovementStart.Invoke(false);
            data.Land();
            yield return new WaitForSeconds(2);
            if (FindAnyObjectByType<TurnUI>() != null)
            {
                FindAnyObjectByType<TurnUI>().StartPlayerTurn(this);
            }
        }
    }

    private void OnKnotEnter(SplineKnotIndex index)
    {
        SplineKnotData data = splineKnotData.splineDatas[index.Spline].knots[index.Knot];
        //Debug.Log($"Entered: S{data.knotIndex.Spline}K{data.knotIndex.Knot}");

        OnMovementUpdate.Invoke(splineKnotAnimator.Step);
    }

    public void PrepareToRoll()
    {
        isRolling = true;

        OnRollStart.Invoke();
    }

    IEnumerator RollSequence()
    {
        allowInput = false;
        OnRollJump.Invoke();

        roll = Random.Range(1, 11);

        yield return new WaitForSeconds(jumpDelay);

        OnRollDisplay.Invoke(roll);

        yield return new WaitForSeconds(resultDelay);

        isRolling = false;
        OnRollEnd.Invoke();

        yield return new WaitForSeconds(startMoveDelay);

        splineKnotAnimator.Animate(roll);

        OnMovementStart.Invoke(true);
        OnMovementUpdate.Invoke(roll);
        allowInput = true;
    }

    public void AllowInput(bool allow)
    {
        allowInput = allow;
    }

    void OnJump()
    {
        if (!allowInput)
            return;


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
        }
    }

    void OnMove(InputValue value)
    {
        if (!allowInput)
            return;

        if (value.Get<Vector2>().y != 0)
            splineKnotAnimator.AddToJunctionIndex(-(int)value.Get<Vector2>().y);
    }

    void OnCancel()
    {
        if (!allowInput)
            return;

        if (!isRolling)
            return;

        isRolling = false;

        if (FindAnyObjectByType<TurnUI>() != null)
        {
            FindAnyObjectByType<TurnUI>().StartPlayerTurn(this);
        }

        OnRollCancel.Invoke();
    }
}
