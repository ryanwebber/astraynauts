using UnityEngine;
using System.Collections;

public class PlanetSurfaceSceneInitializer : MonoBehaviour
{
    [SerializeField]
    private AttachableInputSource inputSource;

    [SerializeField]
    private PlayerInputBinder inputBinder;

    private void Start()
    {
        inputBinder.Bind(inputSource.MainSource);
    }
}
