using UnityEngine;
using System.Collections;
using System.Linq;

public class NavigationService : MonoBehaviour
{
    [SerializeField]
    private NavigationTopology navigationTopology;
    public NavigationTopology NavigationTopology => navigationTopology;

    [SerializeField]
    private GameState gameState;

    private void Awake()
    {
        gameState.OnGameStateInitializationBegin += InitializeNavigationTopology;
        gameState.OnGameStateInitializationEnd += BindNavigationToPlayers;
    }

    private void InitializeNavigationTopology()
    {
        navigationTopology.InitalizeTopology(gameState.World.Bounds.size);

        var traversableUnits = gameState.World.Grid.GetUnits()
            .Where(u => u.unit.ContainsDescriptor<FloorDescriptor>() && !u.unit.ContainsDescriptor<FixtureDescriptor>())
            .Select(u => u.position);

        foreach (var unit in traversableUnits)
            navigationTopology.SetState(unit, NavigationTopology.PathType.TRAVERSABLE);
    }

    private void BindNavigationToPlayers()
    {
        var playerManager = gameState.Services.PlayerManager;
        navigationTopology.SetTargets(playerManager.GetAlivePlayers().Select(player => new Vector2Int(
            Mathf.FloorToInt(player.transform.position.x),
            Mathf.FloorToInt(player.transform.position.y)
        )));
    }
}
