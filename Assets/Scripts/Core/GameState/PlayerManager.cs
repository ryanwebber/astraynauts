using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;

public class PlayerManager : MonoBehaviour
{
    [SerializeField]
    private GameState gameState;

    private List<Player> players;

    public Vector2 ApproximatePlayerPositioning
    {
        get
        {
            Assert.IsTrue(players.Count > 0, "There are no players?");
            return players.Aggregate(Vector2.zero, (accum, player) => accum + player.WorldPosition) / players.Count;
        }
    }

    private void Awake()
    {
        gameState.OnGameStateInitializationBegin += () =>
        {
            players = new List<Player>(gameState.SceneParameters.players);
            foreach (var player in players)
            {
                player.OnPlayerWillSpawn?.Invoke(gameState);
            }
        };

        gameState.OnGameStateInitializationEnd += () =>
        {
            SpawnPlayers();
        };
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
