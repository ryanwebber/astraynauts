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
        public TileDistribution ceilingTiles;

        [SerializeField]
        public TileDistribution northWallTiles;

        [SerializeField]
        public TileDistribution southWallTiles;
    }

    [System.Serializable]
    public class CollisionSettings
    {
        [SerializeField]
        public Tilemap tilemap;

        [SerializeField]
        public TileBase tile;
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

    [SerializeField]
    private CollisionSettings collisionSettings;

    public void LoadWorld(WorldGenerator.WorldLayout layout, System.Action<World> completion)
    {
        StartCoroutine(LoadWorldDistributed(layout, completion));
    }

    private IEnumerator LoadWorldDistributed(WorldGenerator.WorldLayout layout, System.Action<World> completion)
    {
        var loadFloorsOperation = PlaceFloors(layout);
        var loadWallsOperation = PlaceWalls(layout);

        var loader = new SafeLoader(loadFloorsOperation, loadWallsOperation);

        foreach (var y in loader.SparseLoad<YieldInstruction>(null))
        {
            yield return y;
        }
 
        // TODO: Delete me
        foreach (var door in layout.Hallways.SelectMany(h => h.Path).SelectMany(GetScaled))
        {
            floorSettings.tilemap.SetTileFlags(new Vector3Int(door.x, door.y, 0), TileFlags.None);
            floorSettings.tilemap.SetColor(new Vector3Int(door.x, door.y, 0), Color.yellow);
        }

        World world = new World(layout, layoutScale);
        completion?.Invoke(world);
    }

    private IEnumerable<IOperation> PlaceWalls(WorldGenerator.WorldLayout layout)
    {
        var ceilingTileset = wallSettings.ceilingTiles.AsCollection();
        var northWallTileset = wallSettings.northWallTiles.AsCollection();
        var southWallTileset = wallSettings.southWallTiles.AsCollection();
        var tilemap = wallSettings.tilemap;

        HashSet<Vector2Int> allFloors = new HashSet<Vector2Int>();
        HashSet<Vector2Int> rejectPositions = new HashSet<Vector2Int>();

        HashSet<Vector2Int> collisionTiles = new HashSet<Vector2Int>();

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
                foreach (var position in WalkScaled(from: cell * layoutScale + Vector2Int.left, Vector2Int.up).Where(p => !rejectPositions.Contains(p)))
                {
                    yield return new TileAssignment
                    {
                        tile = ceilingTileset.NextValue(),
                        tilemap = tilemap,
                        position = position
                    };

                    collisionTiles.Add(position);
                }
            }

            if (IsEmpty(Vector2Int.right))
            {
                foreach (var position in WalkScaled(from: (cell + Vector2Int.right) * layoutScale, Vector2Int.up).Where(p => !rejectPositions.Contains(p)))
                {
                    yield return new TileAssignment
                    {
                        tile = ceilingTileset.NextValue(),
                        tilemap = tilemap,
                        position = position
                    };

                    collisionTiles.Add(position);
                }
            }

            if (IsEmpty(Vector2Int.down))
            {
                foreach (var position in WalkScaled(from: cell * layoutScale + Vector2Int.down, Vector2Int.right))
                {
                    yield return new TileAssignment
                    {
                        tile = ceilingTileset.NextValue(),
                        tilemap = tilemap,
                        position = position
                    };

                    yield return new TileAssignment
                    {
                        tile = southWallTileset.NextValue(),
                        tilemap = tilemap,
                        position = position + Vector2Int.up
                    };

                    collisionTiles.Add(position);
                }
            }

            if (IsEmpty(Vector2Int.up))
            {
                foreach (var position in WalkScaled(from: (cell + Vector2Int.up) * layoutScale, Vector2Int.right))
                {
                    yield return new TileAssignment
                    {
                        tile = ceilingTileset.NextValue(),
                        tilemap = tilemap,
                        position = position + Vector2Int.up
                    };

                    yield return new TileAssignment
                    {
                        tile = northWallTileset.NextValue(),
                        tilemap = tilemap,
                        position = position
                    };

                    // North wall doesn't want to be overwritten by left/right checked ceil tiles
                    rejectPositions.Add(position);

                    collisionTiles.Add(position);
                }
            }

            if (IsEmpty(Vector2Int.up) && IsEmpty(Vector2Int.right))
            {
                yield return new TileAssignment
                {
                    tile = ceilingTileset.NextValue(),
                    tilemap = tilemap,
                    position = (cell + Vector2Int.one) * layoutScale
                };

                // Upper corners are 2 ceil tiles tall
                yield return new TileAssignment
                {
                    tile = ceilingTileset.NextValue(),
                    tilemap = tilemap,
                    position = (cell + Vector2Int.one) * layoutScale + Vector2Int.up
                };
            }

            if (IsEmpty(Vector2Int.down) && IsEmpty(Vector2Int.right))
            {
                yield return new TileAssignment
                {
                    tile = ceilingTileset.NextValue(),
                    tilemap = tilemap,
                    position = cell * layoutScale + Vector2Int.down + Vector2Int.right * layoutScale
                };
            }

            if (IsEmpty(Vector2Int.down) && IsEmpty(Vector2Int.left))
            {
                yield return new TileAssignment
                {
                    tile = ceilingTileset.NextValue(),
                    tilemap = tilemap,
                    position = cell * layoutScale + Vector2Int.down + Vector2Int.left
                };
            }

            if (IsEmpty(Vector2Int.up) && IsEmpty(Vector2Int.left))
            {
                yield return new TileAssignment
                {
                    tile = ceilingTileset.NextValue(),
                    tilemap = tilemap,
                    position = cell * layoutScale + Vector2Int.up * layoutScale + Vector2Int.left
                };

                // Upper corners are 2 ceil tiles tall
                yield return new TileAssignment
                {
                    tile = ceilingTileset.NextValue(),
                    tilemap = tilemap,
                    position = cell * layoutScale + Vector2Int.up * layoutScale + Vector2Int.up + Vector2Int.left
                };
            }
        }

        var floorCells = layout.Rooms.AllRooms.SelectMany(r => r.GetAllCells());
        var hallwayCells = layout.Hallways.SelectMany(h => h.Path);

        foreach (var floor in Enumerable.Concat(floorCells, hallwayCells))
            allFloors.Add(floor);

        IEnumerable<IOperation> wallPlacement = Enumerable.Concat(floorCells, hallwayCells).SelectMany(GenerateWallsNeighboring);
        IEnumerable<IOperation> collisionPlacement = collisionTiles.Select(position =>
        {
            return (IOperation) new TileAssignment
            {
                position = position,
                tile = collisionSettings.tile,
                tilemap = collisionSettings.tilemap
            };
        });

        return Enumerable.Concat(wallPlacement, collisionPlacement);
    }

    private IEnumerable<IOperation> PlaceFloors(WorldGenerator.WorldLayout layout)
    {   
        var tileset = floorSettings.tiles.AsCollection();

        var floorCells = layout.Rooms.AllRooms.SelectMany(r => r.GetAllCells());
        var hallwayCells = layout.Hallways.SelectMany(h => h.Path);
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
