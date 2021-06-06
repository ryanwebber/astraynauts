using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public class GameManager : MonoBehaviour
{
    [System.Serializable]
    public class Services
    {
        [SerializeField]
        public PlayerManager playerManager;

        [SerializeField]
        public CameraController cameraController;
    }

    public struct Parameters
    {
        public WorldGenerator.Parameters generationParameters;
        public List<Player> players;
    }

    [SerializeField]
    private WorldLoader worldLoader;

    [SerializeField]
    private Services services;

    [NonSerialized]
    private GameState gameState;

    public void LoadGame(Parameters parameters, Action cb)
    {
        var worldLayout = WorldGenerator.Generate(parameters.generationParameters);
        worldLoader.LoadWorld(worldLayout, (world) => {

            // Initialize the game services
            gameState = new GameState(world, services);
            services.playerManager.Initialize(gameState, parameters.players);
            services.cameraController.Initialize(gameState);

            // Spawn the player
            var spawnSection = UnityEngine.Random.Range(0, world.InitialRoom.SectionCount);
            foreach (var player in services.playerManager.GetAlivePlayers())
                services.playerManager.SpawnPlayer(player, world.InitialRoom, spawnSection);

            // Set the alive players as targets for the navigation service
            worldLoader.NavigationTopology.SetTargets(services.playerManager.GetAlivePlayers().Select(player => new Vector2Int(
                Mathf.FloorToInt(player.transform.position.x),
                Mathf.FloorToInt(player.transform.position.y)
            )));

            // Late initialize game services
            services.cameraController.LateInitialize();

            cb?.Invoke();
        });
    }
}
