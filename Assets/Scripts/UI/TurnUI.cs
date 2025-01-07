using System;
using System.Collections;
using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class TurnUI : MonoBehaviour
{
    [SerializeField] private PlayerController currentPlayer;
    private CanvasGroup canvasGroup;

    [Header("States")]
    private bool isShowingBoard;

    [Header("Turn UI References")]
    [SerializeField] private Button diceButton;
    [SerializeField] private Button itemButton;
    [SerializeField] private Button boardButton;


    [Header("Overlay Camera Settings")]
    [SerializeField] private CinemachineCameraOffset overlayCameraOffset;
    private Vector3 originalCameraOffset;
    [SerializeField] private float disableCameraOffset;

    private GameObject lastSelectedButton;


    void Start()
    {
        canvasGroup = GetComponentInChildren<CanvasGroup>();
        canvasGroup.alpha = 0;
        diceButton.onClick.AddListener(OnDiceButtonSelect);
        boardButton.onClick.AddListener(OnBoardButtonSelect);
        originalCameraOffset = overlayCameraOffset.Offset;
        lastSelectedButton = diceButton.gameObject;

        StartPlayerTurn(currentPlayer);

        EventSystem.current.GetComponent<InputSystemUIInputModule>().cancel.action.performed += CancelPerformed;
    }



    public void StartPlayerTurn(PlayerController player)
    {
        currentPlayer = player;
        ShowUI(true);
    }

    private void OnDiceButtonSelect()
    {
        lastSelectedButton = diceButton.gameObject;
        currentPlayer.PrepareToRoll();
        ShowUI(false);
    }

    private void OnBoardButtonSelect()
    {
        lastSelectedButton = boardButton.gameObject;
        SetBoardView(true);
    }

    void SetBoardView(bool view)
    {
        if (FindAnyObjectByType<CameraHandler>() != null)
        {
            FindAnyObjectByType<CameraHandler>().ShowBoard(view);
            ShowUI(!view);
            isShowingBoard = view;
        }
    }

    void ShowUI(bool show)
    {
        DOVirtual.Float(show ? disableCameraOffset : 0, show ? 0 : disableCameraOffset, .4f, CameraOffset);
        canvasGroup.DOFade(show ? 1 : 0, .3f);

        StartCoroutine(EventSystemSelectionDelay());

        IEnumerator EventSystemSelectionDelay()
        {
            yield return new WaitForSeconds(show ? .3f : 0);
            EventSystem.current.SetSelectedGameObject(show ? lastSelectedButton : null);
        }
    }

    private void CancelPerformed(InputAction.CallbackContext context)
    {
        if (isShowingBoard)
        {
            SetBoardView(false);
        }
    }


    void CameraOffset(float x)
    {
        overlayCameraOffset.Offset = originalCameraOffset + new Vector3(x, 0, 0);
    }

}
