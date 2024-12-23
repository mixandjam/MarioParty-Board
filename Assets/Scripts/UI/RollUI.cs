using System;
using TMPro;
using UnityEngine;

public class RollUI : MonoBehaviour
{
    public PlayerController player;

    private TextMeshProUGUI rollTextMesh;

    [Header("Parameters")]
    [SerializeField] private Vector3 textOffset;

    void Start()
    {
        rollTextMesh = GetComponentInChildren<TextMeshProUGUI>();
        rollTextMesh.gameObject.SetActive(false);

        player.OnMovementStart.AddListener(OnRollStart);
        player.OnMovementUpdate.AddListener(OnRollUpdate);
    }

    private void Update()
    {
        float movementBlend = Mathf.Pow(0.5f, Time.deltaTime * 10);
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(player.transform.position + textOffset);
        rollTextMesh.transform.position = Vector3.Lerp(rollTextMesh.transform.position, screenPosition, movementBlend);
    }

    private void OnRollUpdate(int roll)
    {
        rollTextMesh.text = roll.ToString();
    }

    private void OnRollStart(bool active)
    {
        rollTextMesh.gameObject.SetActive(active);
    }
}
