using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class WorldGrid
{
    public class Unit
    {
        private Vector2Int position;
        private WorldGrid grid;

        public Vector2Int Position => position;
        public WorldGrid Grid => grid;

        private Dictionary<Type, Descriptor> descriptors;

        public Unit(WorldGrid grid, Vector2Int position)
        {
            descriptors = new Dictionary<Type, Descriptor>();
            if (grid != null)
                grid.TryRemove(this, position);

            grid.unitMap[position] = this;
            this.grid = grid;
            this.position = position;
        }

        public void AddDescriptor<T>(T descriptor) where T : Descriptor
        {
            RemoveDescriptor<T>();
            descriptor.Bind(this);
            descriptors[typeof(T)] = descriptor;
        }

        public void AddDescriptor(Descriptor descriptor)
        {
            RemoveDescriptor(descriptor);
            descriptor.Bind(this);
            descriptors[descriptor.GetType()] = descriptor;
        }

        public bool RemoveDescriptor<T>() where T : Descriptor
        {
            return descriptors.Remove(typeof(T));
        }

        public bool RemoveDescriptor(Descriptor descriptor)
        {
            return descriptors.Remove(descriptor.GetType());
        }

        public bool TryGetDescriptor<T>(out T descriptor) where T : Descriptor
        {
            if (descriptors.TryGetValue(typeof(T), out Descriptor erasedDescriptor) && erasedDescriptor is T typedDescriptor)
            {
                descriptor = typedDescriptor;
                return true;
            }

            descriptor = default;
            return false;
        }

        public bool ContainsDescriptor<T>() where T : Descriptor
        {
            return descriptors.ContainsKey(typeof(T));
        }

        public IEnumerable<Descriptor> GetDescriptors() => descriptors.Values;
    }

    public abstract class Descriptor
    {
        private Unit unit;
        public Unit Unit => unit;

        public void Bind(Unit unit)
        {
            if (this.unit != null)
                this.unit.RemoveDescriptor(this);

            this.unit = unit;
        }
    }

    public class FloorDescriptor: Descriptor
    {
        public enum Location
        {
            ROOM, HALLWAY
        }

        public readonly Location FloorLocation;

        public FloorDescriptor(Location floorLocation)
        {
            FloorLocation = floorLocation;
        }
    }

    public class WallDescriptor : Descriptor
    {
        public enum Face
        {
            DOWN, UP
        }

        public readonly Face FacingDirection;

        public WallDescriptor(Face facingDirection)
        {
            FacingDirection = facingDirection;
        }
    }

    public class CeilingDescriptor : Descriptor
    {
        public JointHash JointType => JointHash.Directions
            .Where(dir => Unit.Grid.ContainsDescriptor<CeilingDescriptor>(Unit.Position + JointHash.DirectionVector(dir)))
            .Aggregate(new JointHash(), (accum, dir) => accum += dir);
    }

    public class FixtureDescriptor : Descriptor
    {
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

    public bool ContainsDescriptor<T>(Vector2Int position) where T: Descriptor
    {
        if (TryGetUnit(position, out var unit))
            return unit.ContainsDescriptor<T>();

        return false;
    }

    public bool IsEmptySpace(Vector2Int position)
    {
        return !unitMap.ContainsKey(position);
    }

    public IEnumerable<(Vector2Int position, Unit unit)> GetUnits()
    {
        return unitMap.Select(kvp => (position: kvp.Key, unit: kvp.Value));
    }

    public void AddDescriptor<T>(Vector2Int position, T descriptor) where T: Descriptor
    {
        GetOrInsert(position).AddDescriptor(descriptor);
    }

    public Unit this[Vector2Int position]
    {
        get => unitMap[position];
    }

    public Unit GetOrInsert(Vector2Int position)
    {
        if (!unitMap.ContainsKey(position))
            unitMap[position] = new Unit(this, position);

        return unitMap[position];
    }
}
