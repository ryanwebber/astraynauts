using UnityEngine;
using System.Collections;
using Cinemachine;
using System.Linq;
using System.Collections.Generic;

public class WorldSceneInitializer : MonoBehaviour
{
    [SerializeField]
    private GameManager gameManager;

    [Header("Editor Initialization")]

    [SerializeField]
    private WorldShapeParameters defaultWorldShape;

    [SerializeField]
    private AttachableInputSource inputSource;

    [SerializeField]
    private Player player;

    [SerializeField]
    private CinemachineVirtualCamera virtualCamera;

    [SerializeField]
    private Transform cameraTarget;

    [Header("Temporary")]

    [SerializeField]
    private List<Transform> moveToRandomRoom;

    private void Awake()
    {
        GetComponent<SceneInitializer>().RegisterCallback(InitializeScene);
        GetComponent<SceneInitializer>().RegisterEditorSceneSeeder(SeedSceneWithMockData);
    }

    private void InitializeScene(ISceneLoader loader, System.Action callback)
    {
        // TODO: Do player instantiation
        player.InputBinder.Bind(inputSource.MainSource);

        var players = new List<Player>();
        players.Add(player);

        var worldShape = loader.GetContext<WorldShapeParameters>();
        var parameters = new GameManager.Parameters
        {
            generationParameters = worldShape.Parameters,
            players = players
        };

        gameManager.LoadGame(parameters, callback);
    }

    private void SeedSceneWithMockData(IDebugSceneSeeder seeder)
    {
        seeder.AddContext(defaultWorldShape);
    }
}
