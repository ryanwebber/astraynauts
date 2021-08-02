using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PlayerManager : MonoBehaviour
{
    [SerializeField]
    private GameState gameState;

    private List<Player> players;

    private void Awake()
    {
        gameState.OnGameStateInitializationBegin += () =>
            this.players = new List<Player>(gameState.SceneParameters.players);

        gameState.OnGameStateInitializationEnd += () =>
            SpawnPlayers();
    }

    private void SpawnPlayers()
    {
        var teleporter = gameState.World.State.PlayerSpawnTeleporter;
        foreach (var player in GetAlivePlayers())
        {
            var position = teleporter.Center + Random.insideUnitCircle.normalized * 0.2f;
            player.transform.position = position;
        }
    }

    public IEnumerable<Player> GetAlivePlayers()
    {
        return players;
    }
}
