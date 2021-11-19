using UnityEngine;
using System.Collections;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;

using static WorldGenerator;
using UnityEngine.Assertions;

public class WorldLoader : MonoBehaviour
{
    [System.Serializable]
    public class FloorSettings
    {
        [SerializeField]
        public Tilemap tilemap;

        [SerializeField]
        public TileDistribution roomTiles;

        [SerializeField]
        public TileDistribution hallTiles;

        [SerializeField]
        public TileBase portalNorthEast;

        [SerializeField]
        public TileBase portalSouthEast;

        [SerializeField]
        public TileBase portalSouthWest;

        [SerializeField]
        public TileBase portalNorthWest;
    }

    [System.Serializable]
    public class PerimeterSettings
    {
        [SerializeField]
        public Tilemap tilemap;

        [SerializeField]
        public TileBase collisionTile;
    }

    [System.Serializable]
    public class WallSettings
    {
        [SerializeField]
        public Tilemap foregroundTilemap;

        [SerializeField]
        public Tilemap backgroundTilemap;

        [SerializeField]
        public TileBase northCenter;

        [SerializeField]
        public TileBase southCenterUpper;

        [SerializeField]
        public TileBase southCenterLower;

        [SerializeField]
        public TileBase southEastReflexUpper;

        [SerializeField]
        public TileBase southEastReflexLower;

        [SerializeField]
        public TileBase southWestReflexUpper;

        [SerializeField]
        public TileBase southWestReflexLower;

        [SerializeField]
        public TileBase northEastReflex;

        [SerializeField]
        public TileBase northWestReflex;

        [SerializeField]
        public TileBase eastCenter;

        [SerializeField]
        public TileBase westCenter;

        [SerializeField]
        public TileBase northEast;

        [SerializeField]
        public TileBase northWest;

        [SerializeField]
        public TileBase southEastUpper;

        [SerializeField]
        public TileBase southEastLower;

        [SerializeField]
        public TileBase southWestUpper;

        [SerializeField]
        public TileBase southWestLower;
    }

    [System.Serializable]
    public class FixtureSettings
    {
        [SerializeField]
        public Fixture batteryPrefab;
    }

    [System.Serializable]
    public class DoorSettings
    {
        [SerializeField]
        public DoorController horizontalFacingDoorPrefab;

        [SerializeField]
        public Vector2 horizontalFacingDoorOffset;

        [SerializeField]
        public DoorController verticalFacingDoorPrefab;

        [SerializeField]
        public Vector2 verticalFacingDoorOffset;
    }

    [SerializeField]
    private int layoutScale;

    [SerializeField]
    private FloorSettings floorSettings;

    [SerializeField]
    private PerimeterSettings perimeterSettings;

    [SerializeField]
    private WallSettings wallSettings;

    [SerializeField]
    private FixtureSettings fixtureSettings;

    [SerializeField]
    private DoorSettings doorSettings;

    private World temp = null;

    public void LoadWorld(CellMapping layout, System.Action<World> completion)
    {
        StartCoroutine(LoadWorldDistributed(layout, completion));
    }

    private IEnumerator LoadWorldDistributed(CellMapping layout, System.Action<World> completion)
    {
        var layoutGeneration = new World.LayoutGeneration(layout, layoutScale);

        // Create the world grid, describing every unit of the map
        var worldGrid = new WorldGrid(layoutGeneration.ScaledBounds);
        Profile.Debug("Load world descriptors", () => LoadBaseDescriptors(layout, worldGrid));

        // Load the world and generate the tilemaps
        var tileAssignmentLoader = new FrameDistributedLoader(GetTileAssignments(worldGrid));
        foreach (var state in tileAssignmentLoader.SparseLoad())
        {
            Debug.Log($"Loaded {state.operationCount} tiles");
            if (!state.isDone)
                yield return null;
        }

        // Store the world data
        World world = World.Build(layoutGeneration, worldGrid);

        // Decorate the world with doors
        var doorDecoration = new FrameDistributedLoader(GetDoorPopulations(world));
        foreach (var state in doorDecoration.SparseLoad())
        {
            Debug.Log($"Loaded {state.operationCount} doors");
            if (!state.isDone)
                yield return null;
        }

        // Decorate the world with fixtures
        var fixtureDecoration = new FrameDistributedLoader(GetFixturePopulations(world, worldGrid));
        foreach (var state in fixtureDecoration.SparseLoad())
        {
            Debug.Log($"Loaded {state.operationCount} fixtures");
            if (!state.isDone)
                yield return null;
        }

        // Setup the player spawn
        var spawnTeleporter = IterationUtils.TryUntil(2, (i) =>
        {
            var spawnableRooms = world.Components.GetAll<Room>()
                .Where(room => room.Teleporters.Count >= (2 - i))
                .ToArray();

            if (spawnableRooms.Length == 0)
                return null;

            var spawnRoomIndex = Random.Range(0, spawnableRooms.Length);
            var spawnRoom = spawnableRooms[spawnRoomIndex];

            var teleporterIndex = Random.Range(0, spawnRoom.Teleporters.Count);
            return spawnRoom.Teleporters[teleporterIndex];
        });

        Assert.IsNotNull(spawnTeleporter, "Unable to find a player spawn teleporter");
        world.State.PlayerSpawnTeleporter = spawnTeleporter;

        // Setup the accessible world region
        foreach (var room in world.Components.GetAll<Room>())
        {
            // TODO: Don't use all the rooms in the world, we should
            // have doors and this should be small to start
            foreach (var teleporter in room.Teleporters)
            {
                world.State.AccessibleTeleporters.Add(teleporter);
            }    
        }

        // Generation complete!
        completion?.Invoke(world);
        this.temp = world;
    }

    private IEnumerable<IOperation> GetFixturePopulations(World world, WorldGrid grid)
    {
        foreach (var room in world.Layout.cellMapping.Rooms.AllRooms)
        {
            // TODO: Determine battery at a more intelligent rate, ensuring there's one in the
            // spawn room
            if (Random.value < 0.5f)
                continue;

            var sectionIdx = Random.Range(0, room.SectionCount);
            var section = room.GetSection(sectionIdx);
            var xPos = Random.Range(section.xMin, section.xMax - 1);
            var yPos = Random.Range(section.yMin, section.yMax - 1);

            var unit = world.CellToWorldPosition(new Vector2(xPos, yPos)) + Vector2.one * 1.5f;
            yield return new FixtureInitialization<Fixture>(fixtureSettings.batteryPrefab, grid, instance =>
            {
                instance.transform.position = unit;
            });
        }
    }

    private IEnumerable<IOperation> GetDoorPopulations(World world)
    {
        foreach (var door in world.Components.GetAll<Door>())
        {
            if (door.IsHorizontal)
                yield return new DoorInitialization
                {
                    door = door,
                    prefab = doorSettings.horizontalFacingDoorPrefab,
                    offset = doorSettings.horizontalFacingDoorOffset,
                };
            else
                yield return new DoorInitialization
                {
                    door = door,
                    prefab = doorSettings.verticalFacingDoorPrefab,
                    offset = doorSettings.verticalFacingDoorOffset,
                };
        }
    }

    private IEnumerable<IOperation> GetTileAssignments(WorldGrid grid)
    {
        var perimiterBuffer = 1;
        var rules = WorldTileRules.GetRules(floorSettings, perimeterSettings, wallSettings);

        for (int x = grid.Bounds.xMin - perimiterBuffer; x <= grid.Bounds.xMax + perimiterBuffer; x++)
            for (int y = grid.Bounds.yMin - perimiterBuffer; y <= grid.Bounds.yMax + perimiterBuffer; y++)
                foreach (var pair in rules)
                    foreach (var assignment in pair.GetAssignments(grid, new Vector2Int(x, y)))
                        yield return assignment;
    }

    private void LoadBaseDescriptors(CellMapping layout, WorldGrid grid)
    {
        var rooms = layout.Rooms.AllRooms
            .SelectMany(r => r.GetAllCells())
            .Select(c => new Room(World.ExpandCellToUnits(c, layoutScale)));

        foreach (var room in rooms)
        {
            foreach (var unit in room.Units)
            {
                grid.AddDescriptor(unit, new FloorDescriptor(FloorDescriptor.Location.ROOM));
                grid.AddDescriptor(unit, new RoomDescriptor(room));
            }
        }

        var hallways = layout.Hallways
            .SelectMany(h => h.Path)
            .Select(c => new Hallway(World.ExpandCellToUnits(c, layoutScale)));

        foreach (var hallway in hallways)
        {
            foreach (var unit in hallway.Units)
            {
                grid.AddDescriptor(unit, new FloorDescriptor(FloorDescriptor.Location.HALLWAY));
                grid.AddDescriptor(unit, new HallwayDescriptor(hallway));
            }
        }

        var airlockCells = layout.Airlocks
            .SelectMany(a => World.ExpandCellToUnits(a.Cell, layoutScale));

        foreach (var ga in layout.Airlocks)
        {
            var teleporterCenter = ga.Cell * layoutScale + Vector2.one;
            var teleporterRoomAttachment = teleporterCenter + ga.Direction * layoutScale;
            var teleporterRoomAttachmentUnit = new Vector2Int(Mathf.FloorToInt(teleporterRoomAttachment.x), Mathf.FloorToInt(teleporterRoomAttachment.y));
            var teleporterRoom = grid.GetDescriptor<RoomDescriptor>(teleporterRoomAttachmentUnit).Room;

            var teleporter = new Teleporter(center: ga.Cell * layoutScale + Vector2.one, new Direction(ga.Direction), teleporterRoom);
            foreach (var unit in World.ExpandCellToUnits(ga.Cell, layoutScale))
            {
                grid.AddDescriptor(unit, new FloorDescriptor(FloorDescriptor.Location.TELEPORTER));
                grid.AddDescriptor(unit, new TeleporterDescriptor(teleporter));
            }

            teleporterRoom.Teleporters.Add(teleporter);
        }

        foreach (var doorPairing in layout.Hallways.SelectMany(h => h.DoorMapping))
        {
            var connectingRoom = doorPairing.Key;
            var hallwayCell = doorPairing.Value;

            if (grid.TryGetDescriptor<HallwayDescriptor>(World.ExpandCellToAnyUnit(hallwayCell, layoutScale), out var hallwayDescriptor))
            {
                var hallway = hallwayDescriptor.Hallway;
                var door = new Door(World.BoundCellToUnits(hallwayCell, layoutScale).center);

                foreach (var direction in Direction.Orthogonals)
                {
                    var neighboringRoomCell = hallwayCell + direction;
                    if (grid.TryGetDescriptor<RoomDescriptor>(World.ExpandCellToAnyUnit(neighboringRoomCell, layoutScale), out var roomDescriptor))
                    {
                        var gateway = new Gateway(roomDescriptor.Room, hallway, door, direction);
                        hallway.AddGateway(gateway);
                        door.AddGateway(gateway);
                    }
                }

                Assert.IsTrue(door.Gateways.Count > 0);

                foreach (var unit in World.ExpandCellToUnits(hallwayCell, layoutScale))
                {
                    grid.AddDescriptor(unit, new DoorDescriptor(door));
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (temp == null)
            return;

        var bounds = temp.Bounds;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(bounds.center, (Vector2)bounds.size);
    }
}
