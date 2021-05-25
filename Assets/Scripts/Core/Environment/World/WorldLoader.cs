using UnityEngine;
using System.Collections;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class WorldLoader : MonoBehaviour
{
    [System.Serializable]
    public class FloorSettings
    {
        [SerializeField]
        public Tilemap tilemap;

        [SerializeField]
        public TileDistribution tiles;
    }

    [SerializeField]
    private Vector2Int layoutScale;

    [SerializeField]
    private FloorSettings floorSettings;

    public void LoadWorld(WorldGenerator.WorldLayout layout, System.Action completion)
    {
        PlaceFloors(layout);
    }

    private void PlaceFloors(WorldGenerator.WorldLayout layout)
    {
        Debug.Log("Generating floor tiles...");
        
        var tileset = floorSettings.tiles.AsCollection();
        foreach (var room in layout.rooms.AllRooms)
        {
            foreach (var cell in room.GetAllCells())
            {
                foreach (var position in GetScaled(cell))
                {
                    floorSettings.tilemap.SetTile((Vector3Int)position, tileset.NextValue());
                }
            }
        }

        foreach (var hallway in layout.hallways)
        {
            foreach (var cell in hallway.Path)
            {
                foreach (var position in GetScaled(cell))
                {
                    floorSettings.tilemap.SetTile((Vector3Int)position, tileset.NextValue());
                }
            }
        }

        floorSettings.tilemap.CompressBounds();
    }

    private IEnumerable<Vector2Int> GetScaled(Vector2Int cell)
    {
        for (int y = 0; y < layoutScale.y; y++)
        {
            for (int x = 0; x < layoutScale.x; x++)
            {
                yield return cell * layoutScale + new Vector2Int(x, y);
            }
        }
    }
}
