using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public struct TileRulePair
{
    public ITileCondition condition;
    public TileGenerator generator;

    public bool TryGetAssignment(WorldGrid grid, Vector2Int position, out TileAssignment assignment)
    {
        if (generator.layer == null || generator.source == null)
        {
            assignment = default;
            return false;
        }

        if (condition.Evaluate(grid, position))
        {
            assignment = generator.GetAssignment(position);
            return true;
        }

        assignment = default;
        return false;
    }
}
