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
        private PlayerManager playerManager;
        public PlayerManager PlayerManager => playerManager;

        [SerializeField]
        private CameraController cameraController;
        public CameraController CameraController => cameraController;

        [SerializeField]
        private MobManager mobManager;
        public MobManager MobManager => mobManager;
    }

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
