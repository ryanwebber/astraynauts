using UnityEngine;
using System.Collections;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System;

public class WorldLoader : MonoBehaviour
{
    private struct TileAssignment: IOperation
    {
        public TileBase tile;
        public Vector2Int position;
        public Tilemap tilemap;

        public void Perform() => tilemap?.SetTile((Vector3Int)position, tile);
    }

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
        StartCoroutine(LoadWorldDistributed(layout, completion));
    }

    private IEnumerator LoadWorldDistributed(WorldGenerator.WorldLayout layout, System.Action completion)
    {
        var loadFloorsOperation = PlaceFloors(layout);
        var loader = new SafeLoader(loadFloorsOperation);

        foreach (var y in loader.SparseLoad<YieldInstruction>(null))
        {
            yield return y;
        }
    }

    private IEnumerable<IOperation> PlaceFloors(WorldGenerator.WorldLayout layout)
    {   
        var tileset = floorSettings.tiles.AsCollection();
        foreach (var room in layout.rooms.AllRooms)
        {
            foreach (var cell in room.GetAllCells())
            {
                foreach (var position in GetScaled(cell))
                {
                    yield return new TileAssignment
                    {
                        tile = tileset.NextValue(),
                        position = position,
                        tilemap = floorSettings.tilemap
                    };
                }
            }
        }

        foreach (var hallway in layout.hallways)
        {
            foreach (var cell in hallway.Path)
            {
                foreach (var position in GetScaled(cell))
                {
                    yield return new TileAssignment
                    {
                        tile = tileset.NextValue(),
                        position = position,
                        tilemap = floorSettings.tilemap
                    };
                }
            }
        }

        yield return new LambdaOperation(floorSettings.tilemap.CompressBounds);
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
