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

        player.OnRollStart.AddListener(OnRollStart);
        player.OnRollUpdate.AddListener(OnRollUpdate);
    }

    private void Update()
    {
        rollTextMesh.transform.position = Camera.main.WorldToScreenPoint(player.transform.position + textOffset);
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
