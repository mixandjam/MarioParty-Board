using UnityEngine;
using UnityEngine.Splines;
using System.Linq;
using Unity.Mathematics;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Events;

public class SplineKnotAnimate : MonoBehaviour
{
    [SerializeField] private SplineContainer splineContainer;

    [Header("Movement Parameters")]
    [SerializeField] private float moveSpeed = 10;
    [SerializeField] private float movementLerp = 10;
    [SerializeField] private float rotationLerp = 10;
    private int remainingSteps;
    public int Step => remainingSteps;

    [Header("Knot Logic")]
    public SplineKnotIndex currentKnot;
    public SplineKnotIndex nextKnot;
    private IReadOnlyList<SplineKnotIndex> connectedKnots;

    [Header("Interpolation")]
    private float currentT;

    [Header("Junction Parameters")]
    public int junctionIndex = 0;
    public List<SplineKnotIndex> walkableKnots = new List<SplineKnotIndex>();

    [Header("States")]
    public bool isMoving = false;
    public bool inJunction = false;
    public bool Paused = false;

    [Header("Events")]
    [HideInInspector] public UnityEvent<bool> OnEnterJunction;
    [HideInInspector] public UnityEvent<int> OnJunctionSelection;
    [HideInInspector] public UnityEvent<SplineKnotIndex> OnKnotEnter;
    [HideInInspector] public UnityEvent<SplineKnotIndex> OnKnotLand;

    void Start()
    {
        if (splineContainer == null)
        {
            Debug.LogError("Spline Container not assigned!");
            return;
        }

        // Initialize position at first knot
        currentKnot.Knot = 0;
        currentKnot.Spline = 0;
        currentT = 0;
        Spline spline = splineContainer.Splines[currentKnot.Spline];
        nextKnot = new SplineKnotIndex(currentKnot.Spline, (currentKnot.Knot + 1) % spline.Knots.Count());
    }

    private void Update()
    {
        MoveAndRotate();
    }

    public void Animate(int stepAmount = 1)
    {
        if (isMoving)
        {
            Debug.Log("Already animating");
            return;
        }

        remainingSteps = stepAmount;
        StartCoroutine(MoveAlongSpline());
    }

    IEnumerator MoveAlongSpline()
    {
        if (inJunction)
        {
            yield return new WaitUntil(() => inJunction == false);
            OnEnterJunction.Invoke(false);
            SelectJunctionPath(junctionIndex);
        }

        if (Paused)
            yield return new WaitUntil(() => Paused == false);

        isMoving = true;

        Spline spline = splineContainer.Splines[currentKnot.Spline];
        nextKnot = new SplineKnotIndex(currentKnot.Spline, (currentKnot.Knot + 1) % spline.Knots.Count());
        currentT = spline.ConvertIndexUnit(currentKnot.Knot, PathIndexUnit.Knot, PathIndexUnit.Normalized);
        float nextT;

        if (nextKnot.Knot == 0 && spline.Closed)
            nextT = 1f;
        else
            nextT = spline.ConvertIndexUnit(nextKnot.Knot, PathIndexUnit.Knot, PathIndexUnit.Normalized);

        while (currentT != nextT)
        {
            // Move currentT toward nextT using the adjusted speed
            currentT = Mathf.MoveTowards(currentT, nextT, AdjustedMovementSpeed(spline) * Time.deltaTime);
            yield return null;
        }

        if (currentT >= nextT)
        {
            currentKnot = nextKnot;
            nextKnot = new SplineKnotIndex(currentKnot.Spline, (currentKnot.Knot + 1) % spline.Knots.Count());

            if (nextT == 1)
                currentT = 0;

            splineContainer.KnotLinkCollection.TryGetKnotLinks(currentKnot, out connectedKnots);

            if (IsJunctionKnot(currentKnot))
            {
                inJunction = true;
                junctionIndex = 0;
                isMoving = false;
                OnEnterJunction.Invoke(true);
                OnJunctionSelection.Invoke(junctionIndex);
            }
            else
            {
                //Movement only count on non-junction knots
                remainingSteps--;
            }

            OnKnotEnter.Invoke(currentKnot);

            if (IsLastKnot(currentKnot) && connectedKnots != null)
            {
                foreach (SplineKnotIndex connKnot in connectedKnots)
                {
                    if (!IsLastKnot(connKnot))
                    {
                        currentKnot = connKnot;
                        currentT = splineContainer.Splines[currentKnot.Spline].ConvertIndexUnit(connKnot.Knot, PathIndexUnit.Knot, PathIndexUnit.Normalized);
                    }
                }
            }

            if (remainingSteps > 0)
            {
                StartCoroutine(MoveAlongSpline());
            }
            else
            {
                isMoving = false;
                OnKnotLand.Invoke(currentKnot);
            }

        }

    }

    void MoveAndRotate()
    {
        float movementBlend = Mathf.Pow(0.5f, Time.deltaTime * movementLerp);

        Vector3 targetPosition = (Vector3)splineContainer.EvaluatePosition(currentKnot.Spline, currentT);
        transform.position = Vector3.Lerp(targetPosition, transform.position, movementBlend);

        splineContainer.Splines[currentKnot.Spline].Evaluate(currentT, out float3 position, out float3 direction, out float3 up);

        Vector3 worldDirection = splineContainer.transform.TransformDirection(direction);

        if (worldDirection.sqrMagnitude > 0.0001f && isMoving)
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(worldDirection, Vector3.up), rotationLerp * Time.deltaTime);
    }

    public void AddToJunctionIndex(int amount)
    {
        if (!inJunction)
            return;
        junctionIndex = (int)Mathf.Repeat(junctionIndex + amount, walkableKnots.Count);
        OnJunctionSelection.Invoke(junctionIndex);
    }

    public void SelectJunctionPath(int index)
    {
        if (walkableKnots.Count < 1)
            return;

        SplineKnotIndex selectedKnot = walkableKnots[index];
        currentKnot = selectedKnot;

        Spline spline = splineContainer.Splines[currentKnot.Spline];
        nextKnot = new SplineKnotIndex(currentKnot.Spline, (currentKnot.Knot + 1) % spline.Knots.Count());

        currentT = splineContainer.Splines[currentKnot.Spline].ConvertIndexUnit(currentKnot.Knot, PathIndexUnit.Knot, PathIndexUnit.Normalized);

        walkableKnots.Clear();
    }

    public Vector3 GetJunctionPathPosition(int index)
    {
        if (walkableKnots.Count < 1)
            return Vector3.zero;

        SplineKnotIndex walkableKnotIndex = walkableKnots[index];
        Spline walkableSpline = splineContainer.Splines[walkableKnotIndex.Spline];
        SplineKnotIndex nextWalkableKnotIndex = new SplineKnotIndex(walkableKnotIndex.Spline, (walkableKnotIndex.Knot + 1) % walkableSpline.Knots.Count());
        Vector3 knotPosition = (Vector3)walkableSpline.Knots.ToArray()[nextWalkableKnotIndex.Knot].Position + splineContainer.transform.position;
        return knotPosition;
    }

    bool IsJunctionKnot(SplineKnotIndex knotIndex)
    {
        walkableKnots.Clear();

        if (connectedKnots == null || connectedKnots.Count == 0)
            return false;

        int divergingPaths = 0;

        // Check each connected spline
        foreach (SplineKnotIndex connection in connectedKnots)
        {
            var spline = splineContainer.Splines[connection.Spline];

            if (!IsLastKnot(connection))
            {
                divergingPaths++;
                walkableKnots.Add(connection);
            }
        }

        // Sort walkableKnots by spline index number
        walkableKnots.Sort((knot1, knot2) => knot1.Spline.CompareTo(knot2.Spline));

        if (divergingPaths <= 1)
            walkableKnots.Clear();

        // If we have more than one path starting from this knot, it's an origin
        return divergingPaths > 1;
    }


    bool IsLastKnot(SplineKnotIndex knotIndex)
    {
        var spline = splineContainer.Splines[knotIndex.Spline];
        return knotIndex.Knot >= spline.Knots.ToArray().Length - 1 && !splineContainer.Splines[knotIndex.Spline].Closed;
    }

    float AdjustedMovementSpeed(Spline spline)
    {
        // Calculate the total spline length
        float splineLength = spline.GetLength();

        // Adjust speed relative to spline length
        return moveSpeed / splineLength;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if (inJunction)
            Gizmos.DrawSphere(GetJunctionPathPosition(junctionIndex), 1);
    }
}
