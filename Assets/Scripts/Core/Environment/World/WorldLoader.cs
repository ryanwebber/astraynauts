using UnityEngine;
using System.Collections;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;

using static WorldGenerator;
using static WorldGrid;

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
    public class JointMapping
    {
        [SerializeField]
        private JointHash.JointDefinition definition;
        public JointHash.JointDefinition Definition => definition;

        [SerializeField]
        private TileDistribution tiles;
        public TileDistribution Tiles => tiles;
    }

    public struct HullJoint
    {
        public JointHash mountJoint;
        public JointHash directionJoint;

        public (int mountHash, int directionHash) Tuple => (mountHash: mountJoint.hash, directionHash: directionJoint.hash);
    }

    [System.Serializable]
    public class HullMapping
    {
        [SerializeField]
        private JointHash.JointDefinition mountPoints;
        public JointHash.JointDefinition MountJoint => mountPoints;

        [SerializeField]
        private JointHash.JointDefinition continuityPoints;
        public JointHash.JointDefinition DirectionJoint => continuityPoints;

        [SerializeField]
        private TileDistribution tiles;
        public TileDistribution Tiles => tiles;
    }


    [System.Serializable]
    public class CeilingSettings
    {
        [SerializeField]
        public Tilemap tilemap;

        [SerializeField]
        public TileBase defaultTile;

        [SerializeField]
        public List<JointMapping> jointMappings;

        public Dictionary<int, IRandomAccessCollection<TileBase>> ToLookupTable()
        {
            var dict = new Dictionary<int, IRandomAccessCollection<TileBase>>();
            foreach (var jm in jointMappings)
            {
                var hash = jm.Definition.Hash.hash;
                var tiles = jm.Tiles.AsCollection();
                dict[hash] = tiles;
            }

            return dict;
        }
    }

    [System.Serializable]
    public class WallSettings
    {
        [SerializeField]
        public Tilemap tilemap;

        [SerializeField]
        public TileDistribution northWallTiles;

        [SerializeField]
        public TileDistribution southWallTiles;
    }

    [System.Serializable]
    public class PerimiterSettings
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
        public TileDistribution roomTiles;

        [SerializeField]
        public TileDistribution hallTiles;
    }

    [System.Serializable]
    public class HullSettings
    {
        [SerializeField]
        public Tilemap tilemap;

        [SerializeField]
        public List<HullMapping> jointMappings;

        public Dictionary<(int mountHash, int directionHash), IRandomAccessCollection<TileBase>> ToLookupTable()
        {
            var dict = new Dictionary<(int mountHash, int directionHash), IRandomAccessCollection<TileBase>>();
            foreach (var jm in jointMappings)
            {
                var tiles = jm.Tiles.AsCollection();
                var joint = new HullJoint
                {
                    mountJoint = jm.MountJoint.Hash,
                    directionJoint = jm.DirectionJoint.Hash,
                };

                dict[joint.Tuple] = tiles;
            }

            return dict;
        }
    }

    [SerializeField]
    private int layoutScale;

    [SerializeField]
    private FloorSettings floorSettings;

    [SerializeField]
    private WallSettings wallSettings;

    [SerializeField]
    private CeilingSettings ceilingSettings;

    [SerializeField]
    private PerimiterSettings perimiterSettings;

    [SerializeField]
    private HullSettings hullSettings;

    private World temp = null;

    public void LoadWorld(WorldGenerator.WorldLayout layout, System.Action<World> completion)
    {
        StartCoroutine(LoadWorldDistributed(layout, completion));
    }

    private IEnumerator LoadWorldDistributed(WorldLayout layout, System.Action<World> completion)
    {
        // Create the world grid, describing every unit of the map
        var worldGrid = new WorldGrid();
        foreach (var unitAssignment in GetFloorUnits(layout))
            worldGrid.AddDescriptor(unitAssignment.position, unitAssignment.descriptor);

        foreach (var unitAssignment in GetCeilingUnits(layout))
            worldGrid.AddDescriptor(unitAssignment.position, unitAssignment.descriptor);

        foreach (var unitAssignment in GetWallUnits(layout))
            worldGrid.AddDescriptor(unitAssignment.position, unitAssignment.descriptor);

        // Load the world and generate the tilemaps
        int frameCount = 0;
        var loader = new SafeLoader(GetTileAssignments(worldGrid));
        foreach (var y in loader.SparseLoad<YieldInstruction>(null))
        {
            frameCount++;
            yield return y;
        }

        Debug.Log($"Loaded tilemaps in {frameCount} frames");

        // Store the world data
        var originRoomIndex = Random.Range(0, layout.Rooms.AllRooms.Count);
        var originRoom = layout.Rooms.AllRooms[originRoomIndex];
        World world = new World(layout, layoutScale, originRoom);

        // Generation complete!
        completion?.Invoke(world);
        this.temp = world;
    }

    private IEnumerable<IOperation> GetTileAssignments(WorldGrid grid)
    {
        var ceilingTileTable = ceilingSettings.ToLookupTable();
        var hullTileTable = hullSettings.ToLookupTable();
        var northWallTileset = wallSettings.northWallTiles.AsCollection();
        var southWallTileset = wallSettings.southWallTiles.AsCollection();
        var roomTileset = floorSettings.roomTiles.AsCollection();
        var hallTileset = floorSettings.hallTiles.AsCollection();

        TileBase GetCeilingTileForUnit(CeilingDescriptor desciptor)
        {
            if (ceilingTileTable.TryGetValue(desciptor.JointType.hash, out var tileset))
                return tileset.NextValue();
            else
                return ceilingSettings.defaultTile;
        }

        // Generate the floor, ceiling, walls, and collision tiles
        
        foreach (var pair in grid.GetUnits())
        {
            var position = pair.position;
            var unit = pair.unit;

            foreach (var descriptor in unit.GetDescriptors())
            {
                switch (descriptor)
                {
                    case CeilingDescriptor ceilingDescriptor:

                        yield return new TileAssignment
                        {
                            tilemap = wallSettings.tilemap,
                            position = position,
                            tile = GetCeilingTileForUnit(ceilingDescriptor),
                        };


                        // Also throw in a collision tile
                        yield return new TileAssignment
                        {
                            tilemap = perimiterSettings.tilemap,
                            position = position,
                            tile = perimiterSettings.tile
                        };

                        break;

                    case WallDescriptor _:

                        yield return new TileAssignment
                        {
                            tilemap = wallSettings.tilemap,
                            position = position,
                            tile = northWallTileset.NextValue(),
                        };

                        // Also throw in a collision tile
                        yield return new TileAssignment
                        {
                            tilemap = perimiterSettings.tilemap,
                            position = position,
                            tile = perimiterSettings.tile
                        };

                        break;

                    case FloorDescriptor floorDescriptor:

                        yield return new TileAssignment
                        {
                            tilemap = floorSettings.tilemap,
                            position = position,
                            tile = floorDescriptor.FloorLocation == FloorDescriptor.Location.ROOM ?
                                roomTileset.NextValue() : hallTileset.NextValue(),
                        };

                        // Check if we need to throw in the south wall overlapping this tile
                        if (grid.ContainsDescriptor<CeilingDescriptor>(position + Vector2Int.down))
                            yield return new TileAssignment
                            {
                                tilemap = wallSettings.tilemap,
                                position = position,
                                tile = southWallTileset.NextValue(),
                            };

                        break;
                }
            }
        }

        // Generate the spaceship exterior
        var hullJointMapping = GetShipPerimiterUnits(grid);
        foreach (var kvp in hullJointMapping)
        {
            if (hullTileTable.TryGetValue(kvp.Value.Tuple, out var tileset))
            {
                yield return new TileAssignment
                {
                    tilemap = hullSettings.tilemap,
                    position = kvp.Key,
                    tile = tileset.NextValue()
                };
            } 
        }
    }

    private Dictionary<Vector2Int, HullJoint> GetShipPerimiterUnits(WorldGrid grid)
    { 
        var joints = new Dictionary<Vector2Int, HullJoint>();
        foreach (var u in grid.GetUnits())
        {
            if (u.unit.ContainsDescriptor<CeilingDescriptor>())
            {
                foreach (var dir in JointHash.Directions)
                {
                    var potentialPos = u.position + JointHash.DirectionVector(dir);
                    if (grid.IsEmptySpace(potentialPos))
                    {
                        joints.TryGetValue(potentialPos, out var currentJoint);
                        currentJoint.mountJoint += JointHash.Reversed(dir);
                        joints[potentialPos] = currentJoint;

                    }
                }
            }
        }

        foreach (var position in new List<Vector2Int>(joints.Keys))
            foreach (var dir in JointHash.Directions)
                if (joints.ContainsKey(position + JointHash.DirectionVector(dir)))
                {
                    joints.TryGetValue(position, out var joint);
                    joint.directionJoint += dir;
                    joints[position] = joint;
                }

        return joints;
    }

    private IEnumerable<(Vector2Int position, CeilingDescriptor descriptor)> GetCeilingUnits(WorldLayout layout)
    {
        var floorCells = new HashSet<Vector2Int>(Enumerable.Concat(
            // Room cells
            layout.Rooms.AllRooms.SelectMany(r => r.GetAllCells()),

            // Hallway cells
            layout.Hallways.SelectMany(h => h.Path)
        ));

        bool IsEmptySpace(Vector2Int cell, Vector2Int offset) => !floorCells.Contains(cell + offset);
        IEnumerable<Vector2Int> WalkScaled(Vector2Int from, Vector2Int direction)
        {
            for (int i = 0; i < layoutScale; i++)
                yield return from + direction * i;
        }

        IEnumerable<Vector2Int> GetPositions(Vector2Int cell)
        {
            // Orthogonal Positions

            if (IsEmptySpace(cell, Vector2Int.up))
                foreach (var pos in WalkScaled(from: (cell + Vector2Int.up) * layoutScale, Vector2Int.right))
                    yield return pos + Vector2Int.up; // One-cell gap between the floor and the ceiling upwards

            if (IsEmptySpace(cell, Vector2Int.right))
                foreach (var pos in WalkScaled(from: (cell + Vector2Int.right) * layoutScale, Vector2Int.up))
                    yield return pos;

            if (IsEmptySpace(cell, Vector2Int.down))
                foreach (var pos in WalkScaled(from: cell * layoutScale + Vector2Int.down, Vector2Int.right))
                    yield return pos;

            if (IsEmptySpace(cell, Vector2Int.left))
                foreach (var pos in WalkScaled(from: cell * layoutScale + Vector2Int.left, Vector2Int.up))
                    yield return pos;

            // Diagonal Positions

            if (IsEmptySpace(cell, Vector2Int.down) && IsEmptySpace(cell, Vector2Int.right))
                yield return cell * layoutScale + Vector2Int.down + Vector2Int.right * layoutScale;

            if (IsEmptySpace(cell, Vector2Int.down) && IsEmptySpace(cell, Vector2Int.left))
                yield return cell * layoutScale + Vector2Int.down + Vector2Int.left;

            if (IsEmptySpace(cell, Vector2Int.up) && IsEmptySpace(cell, Vector2Int.right))
            {
                // Two-cell tall walls at the upper corners
                yield return (cell + Vector2Int.one) * layoutScale;
                yield return (cell + Vector2Int.one) * layoutScale + Vector2Int.up;
            }

            if (IsEmptySpace(cell, Vector2Int.up) && IsEmptySpace(cell, Vector2Int.left))
            {
                // Two-cell tall walls at the upper corners
                yield return cell * layoutScale + Vector2Int.up * layoutScale + Vector2Int.left;
                yield return cell * layoutScale + Vector2Int.up * layoutScale + Vector2Int.up + Vector2Int.left;
            }
        }

        return floorCells
            .SelectMany(GetPositions)
            .Select(p => (position: p, descriptor: new CeilingDescriptor()));
    }

    private IEnumerable<(Vector2Int position, WallDescriptor descriptor)> GetWallUnits(WorldLayout layout)
    {
        var floorCells = new HashSet<Vector2Int>(Enumerable.Concat(
            // Room cells
            layout.Rooms.AllRooms.SelectMany(r => r.GetAllCells()),

            // Hallway cells
            layout.Hallways.SelectMany(h => h.Path)
        ));

        bool IsEmptySpace(Vector2Int cell, Vector2Int offset) => !floorCells.Contains(cell + offset);
        IEnumerable<Vector2Int> WalkScaled(Vector2Int from, Vector2Int direction)
        {
            for (int i = 0; i < layoutScale; i++)
                yield return from + direction * i;
        }

        IEnumerable<(Vector2Int position, WallDescriptor descriptor)> GetPositions(Vector2Int cell)
        {
            if (IsEmptySpace(cell, Vector2Int.up))
                foreach (var pos in WalkScaled(from: (cell + Vector2Int.up) * layoutScale, Vector2Int.right))
                    yield return (position: pos, new WallDescriptor(WallDescriptor.Face.DOWN));
        }

        return floorCells.SelectMany(GetPositions);
    }

    private IEnumerable<(Vector2Int position, FloorDescriptor descriptor)> GetFloorUnits(WorldLayout layout)
    {
        var roomPositions = layout.Rooms.AllRooms
            .SelectMany(r => r.GetAllCells())
            .SelectMany(c => World.ExpandCellToUnits(c, layoutScale));

        var roomAssignments = roomPositions.Select(position => (position: position, unit: new FloorDescriptor(FloorDescriptor.Location.ROOM)));

        var hallwayCells = layout.Hallways
            .SelectMany(h => h.Path)
            .SelectMany(c => World.ExpandCellToUnits(c, layoutScale));

        var hallwayAssignments = hallwayCells.Select(position => (position: position, unit: new FloorDescriptor(FloorDescriptor.Location.HALLWAY)));
        
        return Enumerable.Concat(roomAssignments, hallwayAssignments);
    }

    private void OnDrawGizmos()
    {
        if (temp == null)
            return;

        Gizmos.color = Color.magenta;
        foreach (var airlock in temp.Layout.Airlocks)
        {
            var cell = airlock.Cell;
            var centerCell = (Vector2)cell + Vector2.one * 0.5f;
            var centerWorld = temp.CellToWorldPosition(centerCell);
            var size = Vector2.one * temp.LayoutScale;
            Gizmos.DrawCube(centerWorld, new Vector3(size.x, size.y, 1));
        }
    }
}
