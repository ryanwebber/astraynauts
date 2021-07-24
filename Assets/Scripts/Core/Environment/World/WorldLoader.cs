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

    [SerializeField]
    private int layoutScale;

    [SerializeField]
    private FloorSettings floorSettings;

    [SerializeField]
    private PerimeterSettings perimeterSettings;

    [SerializeField]
    private WallSettings wallSettings;

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
        var perimiterBuffer = 1;
        var rules = WorldTileRules.GetRules(floorSettings, perimeterSettings, wallSettings);

        for (int x = grid.Bounds.xMin - perimiterBuffer; x <= grid.Bounds.xMax + perimiterBuffer; x++)
            for (int y = grid.Bounds.yMin - perimiterBuffer; y <= grid.Bounds.yMax + perimiterBuffer; y++)
                foreach (var pair in rules)
                    foreach (var assignment in pair.GetAssignments(grid, new Vector2Int(x, y)))
                        yield return assignment;
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

        var dimensions = (Vector2)(temp.Layout.Parameters.CellularDimensions * temp.LayoutScale);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(dimensions * 0.5f, dimensions);
    }
}
