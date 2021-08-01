using System.Collections.Generic;
using UnityEngine;
using static WorldGenerator;

public class World
{
    public readonly WorldGrid Grid;
    public readonly CellMapping CellLayout;
    public readonly int LayoutScale;

    public RectInt Bounds => new RectInt(Vector2Int.zero, (CellLayout.Parameters.CellularDimensions + Vector2Int.one * 3) * LayoutScale);

    public World(CellMapping layout, int layoutScale, WorldGrid grid)
    {
        CellLayout = layout;
        LayoutScale = layoutScale;
        Grid = grid;
    }

    public Vector2 CellToWorldPosition(Vector2 cell)
    {
        return cell * LayoutScale;
    }

    public IEnumerable<Vector2Int> ExpandCellToUnits(Vector2Int cell)
    {
        return ExpandCellToUnits(cell, LayoutScale);
    }

    public static IEnumerable<Vector2Int> ExpandCellToUnits(Vector2Int cell, int layoutScale)
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
