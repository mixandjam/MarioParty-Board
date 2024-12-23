using System.Collections;
using TMPro;
using UnityEngine;

public class DiceRollAnimation : MonoBehaviour
{
    private PlayerController playerController;

    [Header("Rotation Parameters")]
    public float rotationSpeed = 360f;
    public float tiltAmplitude = 15f;
    public float tiltFrequency = 2f;
    private float tiltTime = 0f;
    private TextMeshPro[] numberLabels;

    [Header("Number Parameters")]
    [SerializeField] private float numberAnimationSpeed = .15f;

    [Header("States")]
    public bool isSpinning;

    private void Start()
    {
        playerController = GetComponentInParent<PlayerController>();
        numberLabels = GetComponentsInChildren<TextMeshPro>();

        playerController.OnRollStart.AddListener(OnRollStart);
        playerController.OnRollEnd.AddListener(OnRollEnd);

        gameObject.SetActive(false);

    }


    private void OnRollStart()
    {
        isSpinning = true;
        gameObject.SetActive(true);
        StartCoroutine(RandomNumberVisual());
    }
    private void OnRollEnd(int roll, float delay)
    {
        isSpinning = false;

        transform.eulerAngles = Vector3.zero;

        SetNumbersValue(roll);

        StartCoroutine(FadeOutSequence());

        IEnumerator FadeOutSequence()
        {
            yield return new WaitForSeconds(delay);
            gameObject.SetActive(false);
        }
    }

    IEnumerator RandomNumberVisual()
    {
        if (isSpinning == false)
            yield break;

        int num = Random.Range(1, 11);
        SetNumbersValue(num);
        yield return new WaitForSeconds(numberAnimationSpeed);
        StartCoroutine(RandomNumberVisual());
    }

    public void SetNumbersValue(int value)
    {
        foreach (TextMeshPro p in numberLabels)
        {
            p.text = value.ToString();
        }
    }

    void Update()
    {

        if (!isSpinning)
            return;

        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);

        tiltTime += Time.deltaTime * tiltFrequency;
        float tiltAngle = Mathf.Sin(tiltTime) * tiltAmplitude;

        transform.rotation = Quaternion.Euler(tiltAngle, transform.rotation.eulerAngles.y, 0);
    }
}
