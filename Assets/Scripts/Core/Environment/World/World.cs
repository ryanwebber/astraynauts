using UnityEngine;

public class World
{
    public readonly WorldGenerator.WorldLayout Layout;
    public readonly int LayoutScale;

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
