using UnityEngine;
using System.Collections;

public class WaveManager : MonoBehaviour
{
    [SerializeField]
    private GameState gameState;

    private MobManager MobManager => gameState.Services.MobManager;

    private void Awake()
    {
        gameState.OnGameStateInitializationEnd += () =>
        {

        };
    }
}