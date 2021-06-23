using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using static WorldGenerator;

public class WorldGrid
{
    public enum UnitType
    {
        FLOOR, WALL, CEILING, FIXTURE
    }

    public abstract class Unit
    {
        private Vector2Int position;
        private WorldGrid grid;

        public Vector2Int Position => position;
        public WorldGrid Grid => grid;

        public abstract UnitType Type { get; }

        public void Bind(WorldGrid grid, Vector2Int position)
        {
            if (grid != null)
                grid.TryRemove(this, position);

            grid.unitMap[position] = this;
            this.grid = grid;
            this.position = position;
        }
    }

    public class FloorUnit: Unit
    {
        public enum Location
        {
            ROOM, HALLWAY
        }

        public override UnitType Type => UnitType.FLOOR;

        public readonly Location FloorLocation;

        public FloorUnit(Location floorLocation)
        {
            FloorLocation = floorLocation;
        }
    }

    public class WallUnit: Unit
    {
        public enum Face
        {
            DOWN, UP
        }

        public override UnitType Type => UnitType.WALL;

        public readonly Face FacingDirection;

        public WallUnit(Face facingDirection)
        {
            FacingDirection = facingDirection;
        }
    }

    public class CeilingUnit: Unit
    {
        public override UnitType Type => UnitType.CEILING;

        public JointHash JointType => JointHash.Directions
            .Where(dir => this.Grid.IsType(this.Position + JointHash.DirectionVector(dir), UnitType.CEILING))
            .Aggregate(new JointHash(), (accum, dir) => accum += dir);
    }

    public class FixtureUnit: Unit
    {
        public override UnitType Type => UnitType.FIXTURE;
    }

    private Dictionary<Vector2Int, Unit> unitMap;

    public WorldGrid()
    {
        this.unitMap = new Dictionary<Vector2Int, Unit>();
    }

    public void TryRemove(Unit unit, Vector2Int position)
    {
        if (unitMap.TryGetValue(position, out var discoveredUnit) && discoveredUnit == unit)
            unitMap.Remove(position);
    }

    public bool TryGetUnit(Vector2Int position, out Unit unit)
    {
        return unitMap.TryGetValue(position, out unit);
    }

    public bool TryGetUnit<T>(Vector2Int position, out T unit) where T: Unit
    {
        if (TryGetUnit(position, out var tmp) && tmp is T castedUnit)
        {
            unit = castedUnit;
            return true;
        }

        unit = default;
        return false;
    }

    public bool IsType(Vector2Int position, UnitType type)
    {
        if (TryGetUnit(position, out var unit))
            return unit.Type == type;

        return false;
    }

    public IEnumerable<(Vector2Int position, Unit unit)> GetUnits()
    {
        return unitMap.Select(kvp => (position: kvp.Key, unit: kvp.Value));
    }

    public Unit this[Vector2Int position]
    {
        get => unitMap[position];
        set => value.Bind(this, position);
    }
}
