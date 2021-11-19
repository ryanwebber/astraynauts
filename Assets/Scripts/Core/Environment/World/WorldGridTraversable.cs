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
            bool isFloor = unit.ContainsDescriptor<FloorDescriptor>();
            bool isImpassable =
                unit.ContainsDescriptor<FixtureDescriptor>() || // Fixture is in the way
                (unit.TryGetDescriptor(out DoorDescriptor d) && !d.Door.IsOpen); // Is a door which is closed

            return isFloor && !isImpassable;
        }

        return false;
    }
}
