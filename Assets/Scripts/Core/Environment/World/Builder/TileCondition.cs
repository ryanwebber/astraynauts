using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum Operator
{
    AND, OR
}

public interface ITileCondition
{
    bool Evaluate(WorldGrid grid, Vector2Int position);
}

public struct RelativeTileTypeCondition<T>: ITileCondition where T: WorldGrid.Descriptor
{
    public Vector2Int offset;
    public Func<T, bool> predicate;

    public bool Evaluate(WorldGrid grid, Vector2Int position)
    {
        if (grid.TryGetDescriptor<T>(position + offset, out var descriptor))
            if (predicate == null || predicate.Invoke(descriptor))
                return true;

        return false;
    }
}

public struct AllOfCondition : ITileCondition
{
    private ICollection<ITileCondition> conditions;

    public AllOfCondition(params ITileCondition[] conditions)
    {
        this.conditions = conditions;
    }

    public bool Evaluate(WorldGrid grid, Vector2Int position)
        => conditions.All(condition => condition.Evaluate(grid, position));
}

public struct AnyOfCondition : ITileCondition
{
    private ICollection<ITileCondition> conditions;

    public AnyOfCondition(params ITileCondition[] conditions)
    {
        this.conditions = conditions;
    }

    public bool Evaluate(WorldGrid grid, Vector2Int position)
        => conditions.Any(condition => condition.Evaluate(grid, position));
}

public struct NoneOfCondition : ITileCondition
{
    private ICollection<ITileCondition> conditions;

    public NoneOfCondition(params ITileCondition[] conditions)
    {
        this.conditions = conditions;
    }

    public bool Evaluate(WorldGrid grid, Vector2Int position)
        => !conditions.Any(condition => condition.Evaluate(grid, position));
}
