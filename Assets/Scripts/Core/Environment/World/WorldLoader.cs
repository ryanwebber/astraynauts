using UnityEngine;
using System.Collections;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;

using static WorldGenerator;

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

    private World temp = null;

    public void LoadWorld(WorldGenerator.WorldLayout layout, System.Action<World> completion)
    {
        StartCoroutine(LoadWorldDistributed(layout, completion));
    }

    private IEnumerator LoadWorldDistributed(WorldLayout layout, System.Action<World> completion)
    {
        // Create the world grid, describing every unit of the map
        var worldGrid = new WorldGrid();
        LoadBaseDescriptors(layout, worldGrid);

        // Load the world and generate the tilemaps
        var tileAssignmentLoader = new FrameDistributedLoader(GetTileAssignments(worldGrid));
        foreach (var state in tileAssignmentLoader.SparseLoad())
        {
            Debug.Log($"Loaded {state.operationCount} tiles in {state.frameCount} frames ({state.duration}s).");
            yield return null;
        }

        // Store the world data
        var originRoomIndex = Random.Range(0, layout.Rooms.AllRooms.Count);
        var originRoom = layout.Rooms.AllRooms[originRoomIndex];
        World world = new World(layout, layoutScale, originRoom);

        var fixtureDecoration = new FrameDistributedLoader(GetFixturePopulations(world, worldGrid));
        foreach (var state in fixtureDecoration.SparseLoad())
        {
            Debug.Log($"Loaded {state.operationCount} fixtures in {state.frameCount} frames ({state.duration}s).");
            yield return null;
        }

        // Generation complete!
        completion?.Invoke(world);
        this.temp = world;
    }

    private IEnumerable<IOperation> GetFixturePopulations(World world, WorldGrid grid)
    {
        foreach (var room in world.Layout.Rooms.AllRooms)
        {
            var sectionIdx = Random.Range(0, room.SectionCount);
            var section = room.GetSection(sectionIdx);
            var xPos = Random.Range(section.xMin, section.xMax - 1);
            var yPos = Random.Range(section.yMin, section.yMax - 1);

            Debug.Log($"x={xPos} y:{yPos}");

            var unit = world.CellToWorldPosition(new Vector2(xPos, yPos)) + Vector2.one * 1.5f;
            yield return new FixtureInitialization<Fixture>(fixtureSettings.batteryPrefab, grid, instance =>
            {
                instance.transform.position = unit;
            });
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

    private void LoadBaseDescriptors(WorldLayout layout, WorldGrid grid)
    {
        var roomPositions = layout.Rooms.AllRooms
            .SelectMany(r => r.GetAllCells())
            .SelectMany(c => World.ExpandCellToUnits(c, layoutScale));

        foreach (var position in roomPositions)
            grid.AddDescriptor(position, new FloorDescriptor(FloorDescriptor.Location.ROOM));

        var hallwayCells = layout.Hallways
            .SelectMany(h => h.Path)
            .SelectMany(c => World.ExpandCellToUnits(c, layoutScale));

        foreach (var position in hallwayCells)
            grid.AddDescriptor(position, new FloorDescriptor(FloorDescriptor.Location.HALLWAY));

        var airlockCells = layout.Airlocks
            .SelectMany(a => World.ExpandCellToUnits(a.Cell, layoutScale));

        foreach (var position in airlockCells)
            grid.AddDescriptor(position, new FloorDescriptor(FloorDescriptor.Location.TELEPORTER));
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
