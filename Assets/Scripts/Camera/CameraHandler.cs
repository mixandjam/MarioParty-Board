using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;

public class CameraHandler : MonoBehaviour
{

    public CinemachineCamera defaultCamera;
    public CinemachineCamera zoomCamera;
    public Volume depthOfFieldVolume;

    private bool isZoomed = false;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            ZoomCamera(!isZoomed);
        }

        depthOfFieldVolume.weight = Mathf.Lerp(depthOfFieldVolume.weight, isZoomed ? 1 : -1, 10 * Time.deltaTime);
    }

    public void ZoomCamera(bool zoom)
    {
        defaultCamera.Priority = zoom ? -1 : 1;
        zoomCamera.Priority = zoom ? 1 : -1;
        isZoomed = zoom;
    }

}
