using UnityEngine;

[RequireComponent(typeof(SplineKnotAnimate))]
public class PlayerController : MonoBehaviour
{
    private SplineKnotAnimate splineKnotAnimator;
    [SerializeField] private int roll = 0;

    void Start()
    {
        splineKnotAnimator = GetComponent<SplineKnotAnimate>();
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
