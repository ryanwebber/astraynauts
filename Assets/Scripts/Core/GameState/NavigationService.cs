﻿using UnityEngine;
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
        var world = gameState.World;
        var layout = world.CellLayout;
        navigationTopology.InitalizeTopology(world.Bounds.size);
        foreach (var cell in layout.Hallways.SelectMany(h => h.Path).SelectMany(world.ExpandCellToUnits))
            navigationTopology.SetState(cell, NavigationTopology.PathType.TRAVERSABLE);

        foreach (var cell in layout.Rooms.Layout.Keys.SelectMany(world.ExpandCellToUnits))
            navigationTopology.SetState(cell, NavigationTopology.PathType.TRAVERSABLE);
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
