using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavigationTopology: MonoBehaviour
{
    public enum CellState
    {
        BLOCKED = 0,
        TRAVERSABLE
    }

    public struct Topology
    {
        public Vector2 slope;
        public int height;
    }

    private static readonly Vector2Int[] NeighboringDirections = new Vector2Int[]
    {
        Vector2Int.up,
        Vector2Int.right,
        Vector2Int.down,
        Vector2Int.left,

        new Vector2Int(1, 1),
        new Vector2Int(1, -1),
        new Vector2Int(-1, 1),
        new Vector2Int(-1, -1),
    };

    private Vector2Int dimensions;
    private CellState[,] traversable;
    private Topology[,] topology;

    private List<Vector2Int> targets;
    private Queue<Vector2Int> bfsQueue;
    private HashSet<Vector2Int> bfsSeen;

    private bool isDirty = false;

    public Vector2Int Dimensions => dimensions;
    public bool IsInitialized => topology != null;

    public void InitalizeTopology(Vector2Int dimensions)
    {
        int width = dimensions.x;
        int height = dimensions.y;
        int nCells = width * height;

        this.dimensions = dimensions;
        this.traversable = new CellState[width, height];
        this.topology = new Topology[width, height];
        this.targets = new List<Vector2Int>();
        this.bfsQueue = new Queue<Vector2Int>();
        this.bfsSeen = new HashSet<Vector2Int>();
    }    

    public void SetTargets(IEnumerable<Vector2Int> targets)
    {
        List<Vector2Int> newTargets = new List<Vector2Int>(targets);
        if (newTargets == targets)
            return;

        this.targets.Clear();
        this.targets.AddRange(newTargets);
        this.isDirty = true;
    }

    public void SetCellState(Vector2Int cell, CellState state)
    {
        this.traversable[cell.x, cell.y] = state;
        this.isDirty = true;
    }

    public Topology GetTopology(Vector2Int cell)
    {
        return GetSlopeAndRecalculateIfNeeded(cell.x, cell.y);
    }

    private Topology GetSlopeAndRecalculateIfNeeded(int x, int y)
    {
        if (isDirty)
            Profile.Debug("Recalculate navigation topology", RecalculateTopology);

        return topology[x, y];
    }

    private void RecalculateTopology()
    {
        bfsQueue.Clear();
        bfsSeen.Clear();

        IEnumerable<(Vector2Int cell, Vector2 direction)> GetSteps(Vector2Int cell)
        {
            foreach (var d in NeighboringDirections)
            {
                var c2 = cell + d;
                if (c2.x >= 0 && c2.x < Dimensions.x &&
                    c2.y >= 0 && c2.y < Dimensions.y &&
                    traversable[c2.x, c2.y] == CellState.TRAVERSABLE)
                {
                    yield return (cell: c2, direction: ((Vector2)d * -1f).normalized);
                }
            }
        }

        // Reset the topology
        for (int x = 0; x < Dimensions.x; x++)
            for (int y = 0; y < Dimensions.y; y++)
                topology[x, y] = new Topology { height = int.MaxValue, slope = Vector2.zero };

        List<Vector2Int> currentFringe = new List<Vector2Int>(Dimensions.x + Dimensions.y);
        List<Vector2Int> nextFringe = new List<Vector2Int>(Dimensions.x + Dimensions.y);

        // Add the targets to the current fringe
        foreach (var target in targets)
            currentFringe.Add(target);

        int height = 0;
        while (currentFringe.Count > 0)
        {
            // Update the height of all cells in the current fringe
            foreach (var cell in currentFringe)
            {
                var currentTopology = topology[cell.x, cell.y];
                topology[cell.x, cell.y].slope = Vector2.ClampMagnitude(currentTopology.slope, 1f);
                topology[cell.x, cell.y].height = height;
            }

            // Walk through each neighbor, updating the topology
            foreach (var cell in currentFringe)
            {
                foreach (var step in GetSteps(cell))
                {
                    if (topology[step.cell.x, step.cell.y].height > height)
                        topology[step.cell.x, step.cell.y].slope += step.direction;

                    // Add the cell to the next fringe if we've never seen it before
                    if (!bfsSeen.Contains(step.cell))
                    {
                        nextFringe.Add(step.cell);
                        bfsSeen.Add(step.cell);
                    }
                }
            }

            // Swap the fringe buffers
            var tmp = currentFringe;
            currentFringe = nextFringe;
            nextFringe = tmp;
            nextFringe.Clear();

            height += 1;
        }

        isDirty = false;
    }
}
