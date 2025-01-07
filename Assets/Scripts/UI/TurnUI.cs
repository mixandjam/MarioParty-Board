using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TurnUI : MonoBehaviour
{
    [SerializeField] private PlayerController currentPlayer;
    private CanvasGroup canvasGroup;



    [Header("Turn UI References")]
    [SerializeField] private Button diceButton;
    [SerializeField] private Button itemButton;
    [SerializeField] private Button boardButton;


    [Header("Overlay Camera Settings")]
    [SerializeField] private CinemachineCameraOffset overlayCameraOffset;
    private Vector3 originalCameraOffset;
    [SerializeField] private float disableCameraOffset;


    void Start()
    {
        canvasGroup = GetComponentInChildren<CanvasGroup>();
        canvasGroup.alpha = 0;
        diceButton.onClick.AddListener(OnDiceButtonSelect);
        originalCameraOffset = overlayCameraOffset.Offset;
        StartPlayerTurn(currentPlayer);

    }

    public void StartPlayerTurn(PlayerController player)
    {
        currentPlayer = player;

        canvasGroup.DOFade(1, .3f).OnComplete(OnAnimationComplete);

        overlayCameraOffset.Offset = originalCameraOffset + new Vector3(disableCameraOffset, 0, 0);
        DOVirtual.Float(disableCameraOffset, 0, .2f, CameraOffset);
    }

    void OnAnimationComplete()
    {
        EventSystem.current.SetSelectedGameObject(diceButton.gameObject);
    }

    private void OnDiceButtonSelect()
    {
        currentPlayer.PrepareToRoll();
        EventSystem.current.SetSelectedGameObject(null);
        canvasGroup.DOFade(0, .3f);

        DOVirtual.Float(0, disableCameraOffset, .4f, CameraOffset);
    }

    void CameraOffset(float x)
    {
        overlayCameraOffset.Offset = originalCameraOffset + new Vector3(x, 0, 0);
    }

}
