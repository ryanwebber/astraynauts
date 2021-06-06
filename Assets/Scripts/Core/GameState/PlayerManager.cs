using UnityEngine;
using System.Collections.Generic;
using static WorldGenerator;

public class PlayerManager : MonoBehaviour
{
    private GameState gameState;
    private List<Player> players;

    public void Initialize(GameState gameState, List<Player> players)
    {
        this.gameState = gameState;
        this.players = new List<Player>(players);
    }

    public void SpawnPlayer(Player player, Room room, int section)
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
