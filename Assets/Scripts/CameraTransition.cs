using Unity.Cinemachine;
using UnityEngine;


public class CameraTransition : MonoBehaviour
{
    public CinemachineCamera currentCamera;
    public CinemachineCamera startMenuCamera;
    public CinemachineCamera mainMenuCamera;
    public CinemachineCamera settingsCamera;

    public void UpdateCamera(CinemachineCamera target)
    {
        currentCamera.Priority--;

        currentCamera = target;

        currentCamera.Priority++;
    }
}
