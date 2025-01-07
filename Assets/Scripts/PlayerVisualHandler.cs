
using UnityEngine;
using DG.Tweening;
using UnityEngine.Splines;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Animations.Rigging;

public class PlayerVisualHandler : MonoBehaviour
{
    private Animator animator;
    private PlayerController playerController;
    private SplineKnotAnimate splineKnotAnimator;
    private SplineKnotInstantiate splineKnotData;

    [Header("References")]
    [SerializeField] private Transform playerModel;
    [SerializeField] private Transform playerDice;
    [SerializeField] private Transform junctionVisual;
    [SerializeField] private Transform junctionArrowPrefab;
    private List<GameObject> junctionList;

    [Header("JumpParameters")]
    [SerializeField] private Color selectedJunctionColor;
    [SerializeField] private Color defaultJunctionColor;


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

    [Header("Dynamic Animation")]
    [SerializeField] private Rig headRig;

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        playerController = GetComponentInParent<PlayerController>();
        splineKnotAnimator = GetComponentInParent<SplineKnotAnimate>();
        numberLabels = GetComponentsInChildren<TextMeshPro>();
        playerDice.gameObject.SetActive(false);
        if (FindAnyObjectByType<SplineKnotInstantiate>() != null)
            splineKnotData = FindAnyObjectByType<SplineKnotInstantiate>();

        playerController.OnRollStart.AddListener(OnRollStart);
        playerController.OnRollJump.AddListener(OnRollJump);
        playerController.OnRollCancel.AddListener(OnRollCancel);
        playerController.OnRollDisplay.AddListener(OnRollDisplay);
        playerController.OnRollEnd.AddListener(OnRollEnd);
        playerController.OnMovementStart.AddListener(OnMovementStart);
        splineKnotAnimator.OnEnterJunction.AddListener(OnEnterJunction);
        splineKnotAnimator.OnJunctionSelection.AddListener(OnJunctionSelection);
        splineKnotAnimator.OnKnotLand.AddListener(OnKnotLand);
    }

    private void OnRollStart()
    {
        transform.DOLookAt(Camera.main.transform.position, .35f, AxisConstraint.Y);

        DOVirtual.Float(0, 1, .4f, SetHeadWeight);

        diceSpinning = true;

        StartCoroutine(RandomDiceNumberCoroutine());

        playerDice.gameObject.SetActive(true);
        playerDice.DOScale(0, .3f).From();
    }

    private void OnRollCancel()
    {
        DOVirtual.Float(1, 0, .4f, SetHeadWeight);
        diceSpinning = false;
        playerDice.DOComplete();
        playerDice.DOScale(0, .12f).OnComplete(() => { playerDice.gameObject.SetActive(false); playerDice.transform.localScale = Vector3.one; });

    }

    private void OnRollJump()
    {
        playerModel.DOComplete();
        playerModel.DOJump(transform.position, jumpPower, 1, jumpDuration);
        animator.SetTrigger("RollJump");
    }

    private void OnRollDisplay(int roll)
    {
        playerDice.DOComplete();
        diceSpinning = false;
        SetDiceNumber(roll);
        playerDice.transform.eulerAngles = Vector3.zero;
        Vector3 diceLocalPos = playerDice.localPosition;
        playerDice.DOLocalJump(diceLocalPos, .8f, 1, .25f);
        playerDice.DOPunchScale(Vector3.one / 4, .3f, 10, 1);
    }


    private void OnRollEnd()
    {
        playerDice.gameObject.SetActive(false);
    }

    private void OnMovementStart(bool movement)
    {
        if (movement)
        {
            DOVirtual.Float(1, 0, .2f, SetHeadWeight);
            transform.DOLocalRotate(Vector3.zero, .3f);
        }
        else
        {
            transform.DOLookAt(Camera.main.transform.position, .35f, AxisConstraint.Y);
        }

    }

    void SetHeadWeight(float headWeight)
    {
        headRig.weight = headWeight;
    }

    private void OnKnotLand(SplineKnotIndex index)
    {
        SplineKnotData data = splineKnotData.splineDatas[index.Spline].knots[index.Knot];
        animator.SetTrigger(data.coinGain > 0 ? "Happy" : "Sad");
    }

    private void OnEnterJunction(bool junction)
    {
        animator.SetBool("InJunction", junction);

        if (!junction)
        {
            foreach (GameObject go in junctionList)
            {
                go.transform.DOComplete();
                go.transform.GetChild(0).DOComplete();
                Destroy(go);
            }
            return;
        }
        else
        {

        }

        junctionList = new List<GameObject>();
        junctionVisual.DOComplete();
        junctionVisual.DOScale(0, .2f).From().SetEase(Ease.OutBack);
        for (int i = 0; i < splineKnotAnimator.walkableKnots.Count; i++)
        {
            GameObject junctionObject = Instantiate(junctionArrowPrefab.gameObject, junctionVisual);
            junctionList.Add(junctionObject);
            junctionObject.transform.LookAt(splineKnotAnimator.GetJunctionPathPosition(i), transform.up);
        }

    }


    private void OnJunctionSelection(int junctionIndex)
    {
        for (int i = 0; i < junctionList.Count; i++)
        {
            if (i != junctionIndex)
            {
                junctionList[i].GetComponentInChildren<Renderer>().material.color = defaultJunctionColor;
                junctionList[i].transform.GetChild(0).DOComplete();
                junctionList[i].transform.GetChild(0).DOScale(.2f, .2f);
            }
        }

        junctionList[junctionIndex].GetComponentInChildren<Renderer>().material.color = selectedJunctionColor;
        junctionList[junctionIndex].transform.DOComplete();
        junctionList[junctionIndex].transform.DOPunchScale(Vector3.one / 4, .3f, 10, 1);
        junctionList[junctionIndex].transform.GetChild(0).DOComplete();
        junctionList[junctionIndex].transform.GetChild(0).DOScale(.8f, .3f).SetEase(Ease.OutBack);
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
