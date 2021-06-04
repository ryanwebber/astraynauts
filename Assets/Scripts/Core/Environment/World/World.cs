using UnityEngine;

public class World
{
    public readonly WorldGenerator.WorldLayout Layout;
    public readonly int LayoutScale;

    public Vector2Int UnitSize => (Layout.Parameters.CellularDimensions + Vector2Int.one * 4) * LayoutScale;

    public World(WorldGenerator.WorldLayout layout, int layoutScale)
    {
        Layout = layout;
        LayoutScale = layoutScale;
    }

    public Vector2 CellToWorldPosition(Vector2 cell)
    {
        return cell * LayoutScale;
    }
}
