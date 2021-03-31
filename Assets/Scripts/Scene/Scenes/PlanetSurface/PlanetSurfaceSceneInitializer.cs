using UnityEngine;
using Cinemachine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(SceneInitializer))]
public class PlanetSurfaceSceneInitializer : MonoBehaviour
{
    [SerializeField]
    private PlanetRegionLoader planetRegionLoader;

    [Header("Debugging")]

    [SerializeField]
    TileBase fillTile;

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
        // TODO: instantiate the player controls as a prefab for editor mode
        // and bind the camera in a camera controller
        virtualCamera.Follow = cameraTarget;
        inputBinder.Bind(inputSource.MainSource);

        var surfaceData = loader.GetContext<PlanetSurfaceData>();
        var regionInfo = loader.GetContext<PlanetSurfaceRegionIndex>();

        planetRegionLoader.LoadRegion(regionInfo.regionIndex, surfaceData.planetData);

        callback?.Invoke();
    }

    private void SeedSceneWithMockData(IDebugSceneSeeder seeder)
    {
        var pallet = new FillTerrainInstruction.FillPallet { baseTile = fillTile };
        TerrainData terrainData = new TerrainData(new ITerrainInstruction[] {
            new FillTerrainInstruction(pallet)
        });

        var regionData = new RegionData { terrain = terrainData };
        var planetData = new PlanetData
        {
            regions = new[]
            {
                regionData,
                regionData,
                regionData,
                regionData,
                regionData,
                regionData,
                regionData,
                regionData,
                regionData,
                regionData,
                regionData,
                regionData,
            }
        };

        var planetSurfaceData = new PlanetSurfaceData { planetData = planetData };
        var planetSurfaceRegionIndex = new PlanetSurfaceRegionIndex { regionIndex = 0 };

        seeder.SetContext(planetSurfaceData);
        seeder.SetContext(planetSurfaceRegionIndex);
    }
}
