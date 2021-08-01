using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using static WorldGenerator;

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
        var airlock = gameState.World.Grid.GetUnits()
            .FirstOrDefault(u => u.unit.ContainsDescriptor<AirlockDescriptor>())
            .unit.GetDescriptorOrDefault<AirlockDescriptor>()
            .Airlock;

        foreach (var player in GetAlivePlayers())
        {
            var position = airlock.Center + Random.insideUnitCircle.normalized * 0.2f;
            player.transform.position = position;
        }
    }

    public IEnumerable<Player> GetAlivePlayers()
    {
        return players;
    }
}
