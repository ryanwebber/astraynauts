using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class World
{
    public struct LayoutGeneration
    {
        public readonly CellMapping cellMapping;
        public readonly int scale;

        public LayoutGeneration(CellMapping cellMapping, int scale)
        {
            this.cellMapping = cellMapping;
            this.scale = scale;
        }

        public RectInt ScaledBounds => new RectInt(Vector2Int.zero, (cellMapping.Parameters.CellularDimensions + Vector2Int.one * 3) * scale);
        public RectInt UnscaledBounds => new RectInt(Vector2Int.zero, cellMapping.Parameters.CellularDimensions + Vector2Int.one * 3);
    }

    public class ComponentSet
    {
        private readonly Dictionary<Type, object> typeMap;

        private ComponentSet(Dictionary<Type, object> typeMap)
        {
            this.typeMap = typeMap;
        }

        public ICollection<T> GetAll<T>()
        {
            if (typeMap.TryGetValue(typeof(T), out var obj) && obj is ICollection<T> collection)
                return collection;

            return new T[] { };
        }

        public class Builder
        {
            private WorldGrid grid;
            private Dictionary<Type, object> typeMap;

            public Builder(WorldGrid grid)
            {
                this.grid = grid;
                typeMap = new Dictionary<Type, object>();
            }

            public Builder With<D, T>() where D: WorldGrid.Descriptor, IComponentDescriptor<T>
            {
                return With(unit =>
                {
                    if (unit.TryGetDescriptor<D>(out var d))
                    {
                        return d.Component;
                    }

                    return default;
                });
            }

            public Builder With<T>(Func<WorldGrid.Unit, T> mapFn)
            {
                var components = new HashSet<T>();
                foreach (var u in grid.GetUnits())
                {
                    var extractedComponent = mapFn.Invoke(u.unit);
                    if (extractedComponent != null)
                        components.Add(extractedComponent);
                }

                typeMap[typeof(T)] = (ICollection<T>)components;

                return this;
            }

            public ComponentSet Build()
            {
                return new ComponentSet(typeMap);
            }
        }
    }

    public readonly LayoutGeneration Layout;
    public readonly WorldGrid Grid;
    public readonly ComponentSet Components;
    public readonly WorldState State;

    public RectInt Bounds => Layout.ScaledBounds;

    private World(LayoutGeneration layout, WorldGrid grid, ComponentSet components, WorldState state)
    {
        Layout = layout;
        Grid = grid;
        Components = components;
        State = state;
    }

    public Vector2 CellToWorldPosition(Vector2 cell)
    {
        return cell * Layout.scale;
    }

    public IEnumerable<Vector2Int> ExpandCellToUnits(Vector2Int cell)
    {
        return ExpandCellToUnits(cell, Layout.scale);
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

    public static RectInt BoundCellToUnits(Vector2Int cell, int layoutScale)
    {
        return new RectInt(cell * layoutScale, new Vector2Int(layoutScale, layoutScale));
    }

    public static World Build(CellMapping cellMapping, int scale, WorldGrid grid)
    {
        var layout = new LayoutGeneration(cellMapping, scale);

        var components = Profile.Debug("Generate world components", () =>
        {
            return new ComponentSet.Builder(grid)
                .With<RoomDescriptor, Room>()
                .With<TeleporterDescriptor, Teleporter>()
                .Build();
        });

        Assert.IsTrue(components.GetAll<Room>().Count > 0, "Failed to pull components out of grid correctly");

        var state = new WorldState();

        return new World(
            layout: layout,
            grid: grid,
            components: components,
            state: state
        );
    }
}
