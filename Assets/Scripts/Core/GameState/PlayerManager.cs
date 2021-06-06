using UnityEngine;
using System.Collections.Generic;
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
        var spawnSection = Random.Range(0, gameState.World.InitialRoom.SectionCount);
        foreach (var player in GetAlivePlayers())
            SpawnPlayer(player, gameState.World.InitialRoom, spawnSection);
    }

    private void SpawnPlayer(Player player, Room room, int section)
    {
        var spawnCell = room.GetSection(section).center + Random.insideUnitCircle.normalized;
        var spawnPoint = gameState.World.CellToWorldPosition(spawnCell);
        player.transform.position = spawnPoint;
    }

    public IEnumerable<Player> GetAlivePlayers()
    {
        return players;
    }
}
