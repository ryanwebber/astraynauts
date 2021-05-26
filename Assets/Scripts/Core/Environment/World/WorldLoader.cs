using UnityEngine;
using System.Collections;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;

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
    public class WallSettings
    {
        [SerializeField]
        public Tilemap tilemap;

        [SerializeField]
        public TileBase tiles;
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
    private int layoutScale;

    [SerializeField]
    private FloorSettings floorSettings;

    [SerializeField]
    private WallSettings wallSettings;

    public void LoadWorld(WorldGenerator.WorldLayout layout, System.Action completion)
    {
        StartCoroutine(LoadWorldDistributed(layout, completion));
    }

    private IEnumerator LoadWorldDistributed(WorldGenerator.WorldLayout layout, System.Action completion)
    {
        var loadFloorsOperation = PlaceFloors(layout);
        var loadWallsOperation = PlaceWalls(layout);

        var loader = new SafeLoader(loadFloorsOperation, loadWallsOperation);

        foreach (var y in loader.SparseLoad<YieldInstruction>(null))
        {
            yield return y;
        }
    }

    private IEnumerable<IOperation> PlaceWalls(WorldGenerator.WorldLayout layout)
    {
        var tileset = wallSettings.tiles;
        var tilemap = wallSettings.tilemap;

        HashSet<Vector2Int> allFloors = new HashSet<Vector2Int>();

        IEnumerable<IOperation> GenerateWallsNeighboring(Vector2Int cell)
        {
            bool IsEmpty(Vector2Int direction) => !allFloors.Contains(cell + direction);
            IEnumerable<Vector2Int> WalkScaled(Vector2Int from, Vector2Int direction)
            {
                for (int i = 0; i < layoutScale; i++)
                    yield return from + direction * i;
            }

            if (IsEmpty(Vector2Int.left))
            {
                foreach (var position in WalkScaled(from: cell * layoutScale + Vector2Int.left, Vector2Int.up))
                {
                    yield return new TileAssignment
                    {
                        tile = tileset,
                        tilemap = tilemap,
                        position = position
                    };
                }
            }

            if (IsEmpty(Vector2Int.right))
            {
                foreach (var position in WalkScaled(from: (cell + Vector2Int.right) * layoutScale, Vector2Int.up))
                {
                    yield return new TileAssignment
                    {
                        tile = tileset,
                        tilemap = tilemap,
                        position = position
                    };
                }
            }

            if (IsEmpty(Vector2Int.down))
            {
                foreach (var position in WalkScaled(from: cell * layoutScale + Vector2Int.down, Vector2Int.right))
                {
                    yield return new TileAssignment
                    {
                        tile = tileset,
                        tilemap = tilemap,
                        position = position
                    };
                }
            }

            if (IsEmpty(Vector2Int.up))
            {
                foreach (var position in WalkScaled(from: (cell + Vector2Int.up) * layoutScale, Vector2Int.right))
                {
                    yield return new TileAssignment
                    {
                        tile = tileset,
                        tilemap = tilemap,
                        position = position
                    };
                }
            }
        }

        var floorCells = layout.rooms.AllRooms.SelectMany(r => r.GetAllCells());
        var hallwayCells = layout.hallways.SelectMany(h => h.Path);

        foreach (var floor in Enumerable.Concat(floorCells, hallwayCells))
            allFloors.Add(floor);

        return Enumerable.Concat(floorCells, hallwayCells).SelectMany(GenerateWallsNeighboring);
    }

    private IEnumerable<IOperation> PlaceFloors(WorldGenerator.WorldLayout layout)
    {   
        var tileset = floorSettings.tiles.AsCollection();

        var floorCells = layout.rooms.AllRooms.SelectMany(r => r.GetAllCells());
        var hallwayCells = layout.hallways.SelectMany(h => h.Path);
        var expandedPositions = Enumerable.Concat(floorCells, hallwayCells).SelectMany(GetScaled);

        return expandedPositions.Select(position => (IOperation) new TileAssignment
            {
                tile = tileset.NextValue(),
                position = position,
                tilemap = floorSettings.tilemap
            }
        );
    }

    private IEnumerable<Vector2Int> GetScaled(Vector2Int cell)
    {
        for (int y = 0; y < layoutScale; y++)
        {
            for (int x = 0; x < layoutScale; x++)
            {
                yield return cell * layoutScale + new Vector2Int(x, y);
            }
        }
    }
}
