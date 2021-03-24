using UnityEngine;
using System.Collections;
using UnityEngine.Tilemaps;

public class PlanetRegionTileManager : MonoBehaviour, ITerrainLayerManager
{
    [SerializeField]
    private Tilemap baseTileMap;

    public float CellSize => GetComponent<Grid>().cellSize.x;

    public IMutableTerrainLayer GetLayer()
    {
        return new TerrainLayer(baseTileMap);
    }
}
