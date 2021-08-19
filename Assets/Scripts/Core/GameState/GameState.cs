using System;
using UnityEngine;
using static GameManager;

public class GameState: MonoBehaviour
{
    [System.Serializable]
    public class GameServices
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

        [SerializeField]
        private WaveManager waveManager;
        public WaveManager WaveManager => waveManager;

        [SerializeField]
        private BankService bankService;
        public BankService BankService => bankService;
    }

    public Event OnGameStateInitializationBegin;
    public Event OnGameStateInitializationEnd;

    [SerializeField]
    private GameServices services;
    public GameServices Services => services;

    private World world;
    public World World => world;

    private Parameters parameters;
    public Parameters SceneParameters => parameters;

    public void InitializeInBlock(World world, Parameters parameters, System.Action block)
    {
        this.world = world;
        this.parameters = parameters;

        OnGameStateInitializationBegin?.Invoke();
        block?.Invoke();
        OnGameStateInitializationEnd?.Invoke();
    }
}
