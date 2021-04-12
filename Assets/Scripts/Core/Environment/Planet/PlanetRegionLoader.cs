using UnityEngine;
using System.Collections;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(PlanetRegion))]
public class PlanetRegionLoader : MonoBehaviour
{
    [SerializeField]
    private PlanetRegionEdge edgePrefab;

    [SerializeField]
    private PlanetRegionTileManager tileManager;

    [SerializeField]
    private CameraConstraint cameraConstraint;

    private PlanetRegion region;

    private void Awake()
    {
        region = GetComponent<PlanetRegion>();
    }

    private void OnDestroy()
    {
        DebugUI.Instance.Unset("planet.region");
    }

    private void LoadTiles(TerrainData terrainData)
    {
        var painter = new PlanetRegionTilePainter(tileManager);
        var shape = new PlanetRegionShape(region.Shape, tileManager.CellSize, region.Orientation);

        foreach (var instruction in terrainData.Instructions)
        {
            Profile.Debug($"Executed terrain generation instruction: {instruction}", () =>
            {
                instruction.Paint(shape, painter);
            });
        }
    }

    private void LoadEdges(int regionIndex, PlanetData planetData)
    {
        int idx = 0;
        foreach (var edge in region.Shape.GetEdges())
        {
            var edgeInstance = Instantiate(edgePrefab, transform);
            edgeInstance.SetEdge(edge, idx++);
            edgeInstance.OnCollideWithEdge += GetRegionEdgeHandler(regionIndex, planetData);
        }
    }

    private Event<PlanetRegionEdge> GetRegionEdgeHandler(int currentRegionIndex, PlanetData planetData)
    {
        return edge =>
        {
            var sceneController = SceneController.Instance;
            if (!sceneController.IsSceneLoadInProgress)
            {
                // This should actually use the graph to figure out the right index
                var requestedRegionIndex = planetData.regions.GetNode(currentRegionIndex, edge.Index).Index;

                Debug.Log($"Requested transition to region: {requestedRegionIndex}");

                sceneController.LoadScene(SceneIdentifier.PLANET_SURFACE, unloader =>
                {
                    var surfaceData = new PlanetSurfaceData { planetData = planetData };
                    var surfaceIndex = new PlanetSurfaceRegionIndex { regionIndex = requestedRegionIndex };
                    unloader.SetContext(surfaceData);
                    unloader.SetContext(surfaceIndex);
                });
            }
        };
    }

    public void LoadRegion(int regionIdx, PlanetData planetData)
    {
        DebugUI.Instance.Set("planet.region", regionIdx.ToString());

        region.Orientation = planetData.regions.GetNode(regionIdx).Orientation;
        cameraConstraint.SetBounds(region.Shape.BoundingShape.GetPoints());

        LoadTiles(planetData.regions[regionIdx].terrain);
        LoadEdges(regionIdx, planetData);
    }
}
