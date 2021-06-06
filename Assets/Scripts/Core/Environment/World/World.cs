using UnityEngine;
using static WorldGenerator;

public class World
{
    public readonly Room InitialRoom;
    public readonly WorldGenerator.WorldLayout Layout;
    public readonly int LayoutScale;

    public Vector2Int UnitSize => (Layout.Parameters.CellularDimensions + Vector2Int.one * 4) * LayoutScale;

    public World(WorldLayout layout, int layoutScale, Room initialRoom)
    {
        Layout = layout;
        LayoutScale = layoutScale;
        InitialRoom = initialRoom;
    }

    public Vector2 CellToWorldPosition(Vector2 cell)
    {
        return cell * LayoutScale;
    }
}
