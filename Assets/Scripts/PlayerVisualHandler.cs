
using UnityEngine;
using DG.Tweening;
using UnityEngine.Splines;
using TMPro;
using System.Collections;

public class PlayerVisualHandler : MonoBehaviour
{
    private Animator animator;
    private PlayerController playerController;
    private SplineKnotAnimate splineKnotAnimator;

    [Header("References")]
    [SerializeField] private Transform playerModel;
    [SerializeField] private Transform playerDice;


    [Header("Jump Parameters")]
    [SerializeField] private int jumpPower = 1;
    [SerializeField] private float jumpDuration = .2f;

    [Header("Dice Parameters")]
    public float rotationSpeed = 360f;
    public float tiltAmplitude = 15f;
    public float tiltFrequency = 2f;
    private float tiltTime = 0f;
    [SerializeField] private float numberAnimationSpeed = .15f;
    private TextMeshPro[] numberLabels;

    [Header("States")]
    private bool diceSpinning;

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        playerController = GetComponentInParent<PlayerController>();
        splineKnotAnimator = GetComponentInParent<SplineKnotAnimate>();
        numberLabels = GetComponentsInChildren<TextMeshPro>();
        playerDice.gameObject.SetActive(false);

        playerController.OnRollStart.AddListener(OnRollStart);
        playerController.OnRollEnd.AddListener(OnRollEnd);
        playerController.OnMovementStart.AddListener(OnMovementStart);
        splineKnotAnimator.OnEnterJunction.AddListener(OnEnterJunction);
        splineKnotAnimator.OnKnotLand.AddListener(OnKnotLand);
    }

    private void OnRollStart()
    {
        diceSpinning = true;

        StartCoroutine(RandomDiceNumberCoroutine());

        playerDice.gameObject.SetActive(true);
        playerDice.DOScale(0, .3f).From();
    }

    private void OnRollEnd(int rollValue, float delay)
    {

        playerModel.DOComplete();
        playerModel.DOJump(transform.position, jumpPower, 1, jumpDuration);
        animator.SetTrigger("RollJump");

        StartCoroutine(DelayCoroutine());

        IEnumerator DelayCoroutine()
        {

            yield return new WaitForSeconds(.05f);
            diceSpinning = false;
            SetDiceNumber(rollValue);
            playerDice.transform.eulerAngles = Vector3.zero;

            yield return new WaitForSeconds(delay);
            playerDice.gameObject.SetActive(false);
        }
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

        if (!diceSpinning)
            return;

        SpinDice();

    }

    void SpinDice()
    {

        playerDice.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);

        tiltTime += Time.deltaTime * tiltFrequency;
        float tiltAngle = Mathf.Sin(tiltTime) * tiltAmplitude;

        playerDice.rotation = Quaternion.Euler(tiltAngle, playerDice.rotation.eulerAngles.y, 0);
    }

    IEnumerator RandomDiceNumberCoroutine()
    {
        if (diceSpinning == false)
            yield break;

        int num = Random.Range(1, 11);
        SetDiceNumber(num);
        yield return new WaitForSeconds(numberAnimationSpeed);
        StartCoroutine(RandomDiceNumberCoroutine());
    }

    public void SetDiceNumber(int value)
    {
        foreach (TextMeshPro p in numberLabels)
        {
            p.text = value.ToString();
        }
    }

}
