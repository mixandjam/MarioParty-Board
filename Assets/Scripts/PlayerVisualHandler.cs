
using UnityEngine;
using UnityEngine.Splines;

public class PlayerVisualHandler : MonoBehaviour
{
    private Animator animator;
    private PlayerController playerController;
    private SplineKnotAnimate splineKnotAnimator;

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        playerController = GetComponentInParent<PlayerController>();
        splineKnotAnimator = GetComponentInParent<SplineKnotAnimate>();

        playerController.OnRollStart.AddListener(OnRollStart);
        splineKnotAnimator.OnEnterJunction.AddListener(OnEnterJunction);
        splineKnotAnimator.OnKnotLand.AddListener(OnKnotLand);
    }

    private void OnRollStart(bool arg0)
    {
        if (arg0)
        {
            animator.SetTrigger("Reset");
        }
    }

    private void OnKnotLand(SplineKnotIndex arg0)
    {
        int random = Random.Range(0, 2);
        animator.SetTrigger(random == 0 ? "Happy" : "Sad");
    }

    private void OnEnterJunction(bool junction)
    {
        animator.SetBool("InJunction", junction);
    }

    void Update()
    {

        float speed = splineKnotAnimator.isMoving ? 1 : 0;
        float fadeSpeed = splineKnotAnimator.isMoving ? .1f : .05f;

        animator.SetFloat("Blend", speed, fadeSpeed, Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.J))
        {
            animator.SetTrigger("RollJump");
        }

    }
}
