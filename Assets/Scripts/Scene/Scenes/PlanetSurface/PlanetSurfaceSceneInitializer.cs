using UnityEngine;
using Cinemachine;

public class PlanetSurfaceSceneInitializer : MonoBehaviour
{
    [SerializeField]
    private AttachableInputSource inputSource;

    [SerializeField]
    private PlayerInputBinder inputBinder;

    [SerializeField]
    private CinemachineVirtualCamera virtualCamera;

    [SerializeField]
    private Transform cameraTarget;

    private void Start()
    {
        inputBinder.Bind(inputSource.MainSource);
        virtualCamera.Follow = cameraTarget;
    }
}
