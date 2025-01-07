using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;

public class CameraHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private SplineKnotAnimate splineKnotAnimator;
    [SerializeField] private CinemachineCamera defaultCamera;
    [SerializeField] private CinemachineCamera zoomCamera;
    [SerializeField] private CinemachineCamera junctionCamera;
    [SerializeField] private CinemachineCamera boardCamera;
    // [SerializeField] private Volume depthOfFieldVolume;

    [Header("States")]
    private bool isZoomed = false;

    private void Start()
    {
        splineKnotAnimator = playerController.GetComponent<SplineKnotAnimate>();

        playerController.OnRollStart.AddListener(OnRollStart);
        playerController.OnRollCancel.AddListener(OnRollCancel);
        playerController.OnMovementStart.AddListener(OnMovementStart);
        splineKnotAnimator.OnEnterJunction.AddListener(OnEnterJunction);
    }

    private void OnEnterJunction(bool junction)
    {
        junctionCamera.Priority = junction ? 10 : -1;
    }

    private void OnMovementStart(bool started)
    {
        if (!started)
        {
            StartCoroutine(ZoomSequence());
            IEnumerator ZoomSequence()
            {

                playerController.AllowInput(false);
                ZoomCamera(true);
                yield return new WaitForSeconds(1.5f);
                ZoomCamera(false);
                playerController.AllowInput(true);
            }
        }
        else
        {
            ZoomCamera(false);
        }
    }


    private void OnRollStart()
    {
        ZoomCamera(true);
    }

    private void OnRollCancel()
    {
        ZoomCamera(false);
    }

    private void Update()
    {
        //depthOfFieldVolume.weight = Mathf.Lerp(depthOfFieldVolume.weight, isZoomed ? 1 : -1, 10 * Time.deltaTime);
    }

    public void ZoomCamera(bool zoom)
    {
        defaultCamera.Priority = zoom ? -1 : 1;
        zoomCamera.Priority = zoom ? 1 : -1;
        isZoomed = zoom;
    }

    public void ShowBoard(bool showBoard)
    {
        defaultCamera.Priority = showBoard ? -1 : 1;
        boardCamera.Priority = showBoard ? 1 : -1;
    }

}
