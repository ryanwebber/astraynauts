using UnityEngine;
using System.Collections.Generic;
using System;

public class GameManager : MonoBehaviour
{
    public struct Parameters
    {
        public WorldGenerator.Parameters generationParameters;
        public List<Player> players;
    }

    [SerializeField]
    private WorldLoader worldLoader;

    [SerializeField]
    private GameState gameState;

    public void LoadGame(Parameters parameters, Action cb)
    {
        var worldLayout = WorldGenerator.Generate(parameters.generationParameters);
        worldLoader.LoadWorld(worldLayout, (world) => {
            gameState.InitializeInBlock(world, parameters, () => {
                cb?.Invoke();
            });
        });
    }
}
