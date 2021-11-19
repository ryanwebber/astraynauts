using System;
using UnityEngine;

public class WorldGridTraversable: ITraversableGrid
{
    private WorldGrid grid;

    public WorldGridTraversable(WorldGrid grid)
    {
        this.grid = grid;
    }

    public Vector2Int Dimensions => grid.Bounds.size;

    public bool IsTraversable(Vector2Int position)
    {
        if (grid.TryGetUnit(position, out var unit))
        {
            return unit.ContainsDescriptor<FloorDescriptor>() && !unit.ContainsDescriptor<FixtureDescriptor>();
        }

        return false;
    }
}
