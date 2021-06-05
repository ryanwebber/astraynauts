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
            bool IsTraversable(Vector2Int c) =>
                    c.x >= 0 && c.x < Dimensions.x &&
                    c.y >= 0 && c.y < Dimensions.y &&
                    traversable[c.x, c.y] == CellState.TRAVERSABLE;

            (Vector2Int cell, Vector2 direction) MakeStep(Vector2Int direction) =>
                (cell: cell + direction, direction: (((Vector2)direction) * -1f).normalized);

            bool IsTraversableUp = IsTraversable(cell + Vector2Int.up);
            bool IsTraversableDown = IsTraversable(cell + Vector2Int.down);
            bool IsTraversableLeft = IsTraversable(cell + Vector2Int.left);
            bool IsTraversableRight = IsTraversable(cell + Vector2Int.right);

            if (IsTraversableUp)
                yield return MakeStep(Vector2Int.up);

            if (IsTraversableDown)
                yield return MakeStep(Vector2Int.down);

            if (IsTraversableLeft)
                yield return MakeStep(Vector2Int.left);

            if (IsTraversableRight)
                yield return MakeStep(Vector2Int.right);

            if (IsTraversableUp && IsTraversableRight && IsTraversable(cell + new Vector2Int(1, 1)))
                yield return MakeStep(new Vector2Int(1, 1));

            if (IsTraversableUp && IsTraversableLeft && IsTraversable(cell + new Vector2Int(-1, 1)))
                yield return MakeStep(new Vector2Int(-1, 1));

            if (IsTraversableDown && IsTraversableRight && IsTraversable(cell + new Vector2Int(1, -1)))
                yield return MakeStep(new Vector2Int(1, -1));

            if (IsTraversableDown && IsTraversableLeft && IsTraversable(cell + new Vector2Int(-1, -1)))
                yield return MakeStep(new Vector2Int(-1, -1));
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
