using UnityEngine;
using System.Collections;
using Cinemachine;
using System.Linq;
using System.Collections.Generic;

public class WorldSceneInitializer : MonoBehaviour
{
    [SerializeField]
    private WorldLoader worldLoader;

    [Header("Editor Initialization")]

    [SerializeField]
    private WorldShapeParameters defaultWorldShape;

    [SerializeField]
    private AttachableInputSource inputSource;

    [SerializeField]
    private PlayerInputBinder inputBinder;

    [SerializeField]
    private CinemachineVirtualCamera virtualCamera;

    [SerializeField]
    private Transform cameraTarget;

    private void Awake()
    {
        GetComponent<SceneInitializer>().RegisterCallback(InitializeScene);
        GetComponent<SceneInitializer>().RegisterEditorSceneSeeder(SeedSceneWithMockData);
    }

    private void InitializeScene(ISceneLoader loader, System.Action callback)
    {
        var worldShape = loader.GetContext<WorldShapeParameters>();
        var worldLayout = WorldGenerator.Generate(worldShape.Parameters);
        worldLoader.LoadWorld(worldLayout, (world) => {

            virtualCamera.PreviousStateIsValid = false;
            virtualCamera.Follow = cameraTarget;

            inputBinder.Bind(inputSource.MainSource);

            var rooms = worldLayout.Rooms.AllRooms.ToList();
            var spawnRoom = rooms[Random.Range(0, rooms.Count)];
            var spawnSection = spawnRoom.GetSection(Random.Range(0, spawnRoom.SectionCount));

            var spawnPosition = world.CellToWorldPosition(spawnSection.center);
            cameraTarget.position = spawnPosition;

            callback?.Invoke();
        });
    }

    private void SeedSceneWithMockData(IDebugSceneSeeder seeder)
    {
        seeder.AddContext(defaultWorldShape);
    }
}
