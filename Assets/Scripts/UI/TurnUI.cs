using UnityEngine.Events;
using System.Collections;
using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using TMPro;
using System;

public class TurnUI : MonoBehaviour
{
    [SerializeField] private PlayerController currentPlayer;
    [SerializeField] private CanvasGroup actionsCanvasGroup;
    [SerializeField] private CanvasGroup rollCanvasGroup;
    [SerializeField] private CanvasGroup starPurchasCanvasGroup;

    [Header("Coin and Star References")]
    [SerializeField] private TextMeshProUGUI startCountLabel;
    [SerializeField] private TextMeshProUGUI coinCountLabel;

    [Header("States")]
    private bool isShowingBoard;

    [Header("Turn UI References")]
    [SerializeField] private Button diceButton;
    [SerializeField] private Button itemButton;
    [SerializeField] private Button boardButton;

    [Header("Star Purchase UI References")]
    [SerializeField] private Button starConfirmButton;
    public Button StarButton => starConfirmButton;
    [SerializeField] private Button starCancelButton;
    public Button CancelStarButton => starCancelButton;


    [Header("Overlay Camera Settings")]
    [SerializeField] private CinemachineCameraOffset overlayCameraOffset;
    private Vector3 originalCameraOffset;
    [SerializeField] private float disableCameraOffset;

    private GameObject lastSelectedButton;
    private PlayerStats currentPlayerStats;


    void Awake()
    {
        actionsCanvasGroup.alpha = 0;
        diceButton.onClick.AddListener(OnDiceButtonSelect);
        boardButton.onClick.AddListener(OnBoardButtonSelect);
        originalCameraOffset = overlayCameraOffset.Offset;
        lastSelectedButton = diceButton.gameObject;
        currentPlayerStats = currentPlayer.GetComponent<PlayerStats>();
        currentPlayerStats.OnInitialize.AddListener(UpdatePlayerStats);
        currentPlayerStats.OnAnimation.AddListener(StatAnimation);

        StartPlayerTurn(currentPlayer);

        EventSystem.current.GetComponent<InputSystemUIInputModule>().cancel.action.performed += CancelPerformed;
    }

    private void StatAnimation(int coinCount)
    {
        coinCountLabel.text = coinCount.ToString();
    }

    private void UpdatePlayerStats()
    {
        startCountLabel.text = currentPlayerStats.Stars.ToString();
        coinCountLabel.text = currentPlayerStats.Coins.ToString();
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
        actionsCanvasGroup.DOFade(show ? 1 : 0, .3f);

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

    public void FadeRollText(bool fadeText)
    {
        rollCanvasGroup.DOFade(fadeText ? 0 : 1, .3f);
    }

    public void ShowStarPurchaseUI(bool show)
    {
        //FadeRollText(show);
        starPurchasCanvasGroup.DOFade(show ? 1 : 0, .2f);
        if (show)
            EventSystem.current.SetSelectedGameObject(starConfirmButton.gameObject);
        else
            EventSystem.current.SetSelectedGameObject(null);
    }

}
