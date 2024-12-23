using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;

public class CameraHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private CinemachineCamera defaultCamera;
    [SerializeField] private CinemachineCamera zoomCamera;
    [SerializeField] private Volume depthOfFieldVolume;

    [Header("States")]
    private bool isZoomed = false;

    private void Start()
    {
        playerController.OnRollStart.AddListener(call: OnRollStart);
        playerController.OnRollEnd.AddListener(OnRollEnd);
        playerController.OnMovementStart.AddListener(OnMovementStart);
    }

    private void OnMovementStart(bool started)
    {
        if (!started)
        {
            StartCoroutine(ZoomSequence());
            IEnumerator ZoomSequence()
            {
                ZoomCamera(true);
                yield return new WaitForSeconds(1.5f);
                ZoomCamera(false);
            }
        }
    }

    private void OnRollEnd(int arg0, float arg1)
    {
        StartCoroutine(ZoomSequence());

        IEnumerator ZoomSequence()
        {
            yield return new WaitForSeconds(arg1);
            ZoomCamera(false);
        }
    }

    private void OnRollStart()
    {
        ZoomCamera(true);
    }

    private void Update()
    {
        depthOfFieldVolume.weight = Mathf.Lerp(depthOfFieldVolume.weight, isZoomed ? 1 : -1, 10 * Time.deltaTime);
    }

    public void ZoomCamera(bool zoom)
    {
        defaultCamera.Priority = zoom ? -1 : 1;
        zoomCamera.Priority = zoom ? 1 : -1;
        isZoomed = zoom;
    }

}
