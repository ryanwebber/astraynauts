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
        GetComponent<SceneInitializer>().RegisterEditorCallback(InitializeSceneForDebugMode);
    }

    private void InitializeScene(ISceneLoader loader, System.Action callback)
    {
        if (loader.TryGetContext<PlanetSurfaceData>(out var surfaceData) &&
            loader.TryGetContext<PlanetSurfaceRegionIndex>(out var regionInfo))
        {
            planetRegionLoader.LoadRegion(regionInfo.regionIndex, surfaceData.planetData);
        }
        else
        {
            throw new System.Exception("Unexpected missing data. Unable to load planet surface scene");
        }

        callback?.Invoke();
    }

    private void InitializeSceneForDebugMode()
    {
        inputBinder.Bind(inputSource.MainSource);
        virtualCamera.Follow = cameraTarget;

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

        planetRegionLoader.LoadRegion(0, planetData);
    }
}
