using System;
using System.Collections.Generic;
using UnityEngine.Assertions;

public class PlanetRegionGraph<T>
{
    private struct StaticNode
    {
        public PlanetRegionOrientation orientation;
        public int[] neighbors;
    }

    // Labels faces of the dodecahedron 0-11
    // Each face has 5 sizes - indexed clockwise starting
    // from the first edge after the horizontally-centered
    // point (ie. 1pm-ish for NorthFacing and 7pm-ish for
    // SouthFacing faces).
    //
    // The indexes of the faces have no bearing otherwise
    // on their position.
    private static readonly StaticNode[] MAPPING = new[]
    {
        new StaticNode {
            // 0
            orientation = PlanetRegionOrientation.SouthFacing,
            neighbors = new int[] { 1, 8, 9, 11, 3 }
        },
        new StaticNode {
            // 1
            orientation = PlanetRegionOrientation.SouthFacing,
            neighbors = new int[] { 2, 6, 8, 0, 3 }
        },
        new StaticNode {
            // 2
            orientation = PlanetRegionOrientation.SouthFacing,
            neighbors = new int[] { 3, 4, 5, 6, 1 }
        },
        new StaticNode {
            // 3
            orientation = PlanetRegionOrientation.SouthFacing,
            neighbors = new int[] { 1, 0, 11, 4, 2 }
        },
        new StaticNode {
            // 4
            orientation = PlanetRegionOrientation.SouthFacing,
            neighbors = new int[] { 3, 11, 10, 5, 2 }
        },
        new StaticNode {
            // 5
            orientation = PlanetRegionOrientation.NorthFacing,
            neighbors = new int[] { 7, 6, 2, 4, 10 }
        },
        new StaticNode {
            // 6
            orientation = PlanetRegionOrientation.SouthFacing,
            neighbors = new int[] { 2, 5, 7, 8, 1 }
        },
        new StaticNode {
            // 7
            orientation = PlanetRegionOrientation.NorthFacing,
            neighbors = new int[] { 9, 8, 6, 5, 10 }
        },
        new StaticNode {
            // 8
            orientation = PlanetRegionOrientation.NorthFacing,
            neighbors = new int[] { 9, 0, 1, 6, 7 }
        },
        new StaticNode {
            // 9
            orientation = PlanetRegionOrientation.NorthFacing,
            neighbors = new int[] { 10, 11, 0, 8, 7 }
        },
        new StaticNode {
            // 10
            orientation = PlanetRegionOrientation.NorthFacing,
            neighbors = new int[] { 7, 5, 4, 11, 9 }
        },
        new StaticNode {
            // 11
            orientation = PlanetRegionOrientation.NorthFacing,
            neighbors = new int[] { 10, 4, 3, 0, 9 }
        },
    };

    public static int NodeCount => MAPPING.Length;

    private PlanetRegionNode<T>[] nodes;
    public IEnumerable<PlanetRegionNode<T>> Nodes => nodes;

    private PlanetRegionGraph(Func<int, T> dataSource)
    {
        nodes = new PlanetRegionNode<T>[MAPPING.Length];
        for (int i = 0; i < nodes.Length; i++)
        {
            var data = dataSource.Invoke(i);
            var orientation = MAPPING[i].orientation;
            nodes[i] = new PlanetRegionNode<T>(i, this, orientation, data);
        }
    }

    public PlanetRegionNode<T> GetNode(int index)
    {
        Assert.IsTrue(index < NodeCount);
        return nodes[index];
    }

    public PlanetRegionNode<T> GetNode(int index, int neighbor)
    {
        Assert.IsTrue(neighbor < 5);
        return GetNode(MAPPING[index].neighbors[neighbor]);
    }

    public T this[int index]
    {
        get => GetNode(index).Data;
    }

    public T this[int index, int neighbor]
    {
        get => GetNode(index, neighbor).Data;
    }

    public static PlanetRegionGraph<T> Make(Func<int, T> dataSource)
    {
        return new PlanetRegionGraph<T>(dataSource);
    }
}
