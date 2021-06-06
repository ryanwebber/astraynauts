using System.Collections.Generic;
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
