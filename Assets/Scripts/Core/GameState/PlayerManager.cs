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
            var alivePlayers = players.Where(player => player.State.IsAlive).ToList();
            var releventPlayers = alivePlayers.Count == 0 ? players : alivePlayers;

            Assert.IsTrue(releventPlayers.Count > 0, "There are no players?");

            return releventPlayers.Aggregate(Vector2.zero, (accum, player) => accum + player.WorldPosition) / releventPlayers.Count;
        }
    }

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
