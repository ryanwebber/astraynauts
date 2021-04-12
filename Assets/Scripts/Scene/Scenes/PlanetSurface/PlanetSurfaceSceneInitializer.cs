﻿using UnityEngine;
using Cinemachine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(SceneInitializer))]
public class PlanetSurfaceSceneInitializer : MonoBehaviour
{
    [System.Serializable]
    public class WeightedPallet: IWeightedElement<TileBase>
    {
        [SerializeField]
        private int weight;
        public int Weight => weight;

        [SerializeField]
        private TileBase tile;
        public TileBase Value => tile;
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
    private WeightedPallet[] fillTileset;

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
        var pallet = new FillTerrainInstruction.FillPallet { baseTiles = new RandomAccessCollection<TileBase>(fillTileset) };
        TerrainData terrainData = new TerrainData(new ITerrainInstruction[] {
            new FillTerrainInstruction(pallet)
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
