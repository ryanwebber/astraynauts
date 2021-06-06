using UnityEngine;
using System.Collections;
using Cinemachine;
using System.Linq;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    private CinemachineVirtualCamera virtualCamera;

    private GameState gameState;

    public void Initialize(GameState gameState)
    {
        this.gameState = gameState;
    }

    public void LateInitialize()
    {
        var player = this.gameState.Services.playerManager.GetAlivePlayers().First();
        virtualCamera.PreviousStateIsValid = false;
        virtualCamera.Follow = player.transform;

        Debug.Log($"Assigning vCam follow target to be {player.name}");
    }
}
