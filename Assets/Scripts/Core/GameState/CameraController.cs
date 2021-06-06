using UnityEngine;
using System.Collections;
using Cinemachine;
using System.Linq;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    private CinemachineVirtualCamera virtualCamera;

    [SerializeField]
    private GameState gameState;

    private void Awake()
    {
        gameState.OnGameStateInitializationEnd += () =>
        {
            var player = gameState.Services.PlayerManager.GetAlivePlayers().First();
            virtualCamera.PreviousStateIsValid = false;
            virtualCamera.Follow = player.transform;
        };
    }
}
