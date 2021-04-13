using UnityEngine;
using Cinemachine;
using UnityEngine.Tilemaps;
using System.Linq;

[RequireComponent(typeof(SceneInitializer))]
public class PlanetSurfaceSceneInitializer : MonoBehaviour
{
    [System.Serializable]
    public class WeightedPallet: IWeightedElement<SerializedTilemap>
    {
        [SerializeField]
        private int weight;
        public int Weight => weight;

        [SerializeField]
        private SerializedTilemap tilemap;
        public SerializedTilemap Value => tilemap;
    }

    [SerializeField]
    private PlanetRegionLoader planetRegionLoader;

    [Header("Debugging")]

    [SerializeField]
    private AttachableInputSource inputSource;

    [SerializeField]
    private PlayerInputBinder inputBinder;

    [SerializeField]
    private CinemachineVirtualCamera virtualCamera;

    [SerializeField]
    private Transform cameraTarget;

    [SerializeField]
    private TileBase fillTile;

    [SerializeField]
    private float noiseDensity = 1f;

    [SerializeField]
    private WeightedPallet[] noiseTilesets;

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
        var fillPallet = new FillTerrainInstruction.Pallet { baseTile = fillTile };
        var noisePallet = new AddNoiseTerrainInstruction.Pallet
        {
            templates = new RandomAccessCollection<TilemapTemplate>(
                noiseTilesets.Select(e => (Weight: e.Weight, Value: e.Value.Template)).ToArray()
            )
        };

        TerrainData terrainData = new TerrainData(new ITerrainInstruction[] {
            new FillTerrainInstruction(fillPallet),
            new AddNoiseTerrainInstruction(noisePallet, noiseDensity),
        });

        var regionData = new RegionData { terrain = terrainData };
        var planetData = new PlanetData
        {
            regions = PlanetRegionGraph<RegionData>.Make(_ => regionData)
        };

        var planetSurfaceData = new PlanetSurfaceData { planetData = planetData };
        var planetSurfaceRegionIndex = new PlanetSurfaceRegionIndex { regionIndex = 0 };

        seeder.SetContext(planetSurfaceData);
        seeder.SetContext(planetSurfaceRegionIndex);
    }
}
