
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

        playerController.OnMovementStart.AddListener(OnMovementStart);
        playerController.OnRollEnd.AddListener(OnRollEnd);
        splineKnotAnimator.OnEnterJunction.AddListener(OnEnterJunction);
        splineKnotAnimator.OnKnotLand.AddListener(OnKnotLand);
    }

    private void OnRollEnd(int arg0, float arg1)
    {
        animator.SetTrigger("RollJump");
    }

    private void OnMovementStart(bool arg0)
    {
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

    }
}
