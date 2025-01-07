using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class RollUI : MonoBehaviour
{
    public PlayerController player;
    public Transform playerDice;

    private TextMeshProUGUI rollTextMesh;
    public AnimationCurve scaleEase;

    [Header("Parameters")]
    [SerializeField] private Vector3 textOffset;
    [SerializeField] private float followSmoothness = 5;

    private bool rolling = false;

    void Start()
    {
        rollTextMesh = GetComponentInChildren<TextMeshProUGUI>();
        rollTextMesh.gameObject.SetActive(false);

        player.OnRollEnd.AddListener(OnRollEnd);
        player.OnRollDisplay.AddListener(OnRollUpdate);
        player.OnMovementStart.AddListener(OnMovementUpdate);
        player.OnMovementUpdate.AddListener(OnRollUpdate);
    }

    private void OnMovementUpdate(bool arg0)
    {
        rolling = false;
    }

    private void OnEnable()
    {
        rolling = true;
    }

    private void LateUpdate()
    {
        float movementBlend = Mathf.Pow(0.5f, Time.deltaTime * followSmoothness);
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(rolling ? playerDice.position : player.transform.position + textOffset);
        rollTextMesh.transform.position = Vector3.Lerp(rollTextMesh.transform.position, screenPosition, movementBlend);
    }

    private void OnRollUpdate(int roll)
    {
        if (roll == 0)
            rollTextMesh.gameObject.SetActive(false);
        rollTextMesh.text = roll.ToString();
    }

    private void OnRollEnd()
    {
        rollTextMesh.gameObject.SetActive(true);

        rollTextMesh.transform.DOComplete();
        rollTextMesh.transform.DOScale(0, .2f).From().SetEase(scaleEase);
    }
}
