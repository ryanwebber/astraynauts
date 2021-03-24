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

    private void Start()
    {
        LoadRegion();
    }

    private void LoadRegion()
    {
        LoadTiles();
        LoadEdges();
    }

    private void LoadTiles()
    {
        var pallet = new FillTerrainInstruction.FillPallet { baseTile = fillTile };
        TerrainData terrainData = new TerrainData(new ITerrainInstruction[] {
            new FillTerrainInstruction(pallet)
        });

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
}
