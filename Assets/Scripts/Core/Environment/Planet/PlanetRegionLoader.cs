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

    [Header("Temporary")]

    [SerializeField]
    private TileBase fillTile;

    [SerializeField]
    private TileBase voidTile;

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
        var shape = new PlanetRegionShape(region.Bounds, tileManager.CellSize);

        foreach (var instruction in terrainData.Instructions)
        {
            Profile.Debug($"Executed terrain generation instruction: {instruction}", () =>
            {
                instruction.Paint(shape, painter);
            });
        }
    }

    private void LoadEdges()
    {
        foreach (var edge in region.Bounds.GetEdges())
        {
            var edgeInstance = Instantiate(edgePrefab, transform);
            edgeInstance.SetEdge(edge);
        }
    }

    public void LoadRegion(int region, PlanetData planetData)
    {
        DebugUI.Instance.Set("planet.region", region.ToString());

        LoadTiles(planetData.regions[region].terrain);
        LoadEdges();
    }
}
