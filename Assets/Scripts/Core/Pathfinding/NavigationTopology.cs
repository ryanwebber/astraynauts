using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NavigationTopology: MonoBehaviour
{
    public enum PathType
    {
        BLOCKED = 0,
        TRAVERSABLE
    }

    public struct Topology
    {
        public Vector2 slope;
        public int height;

        public Vector2 ClampedSlope => Vector2.ClampMagnitude(slope, 1f);
    }

    private class SteppableTopologyGenerator
    {
        private List<Vector2Int> fringe;
        private List<Vector2Int> fringeQueue;
        private HashSet<Vector2Int> ignoreSet;
        private HashSet<Vector2Int> seenSet;
        private int currentHeight;

        private Vector2Int dimensions;
        private Topology[,] topology;
        private PathType[,] traversable;

        public int CurrentHeight => currentHeight;
        public bool IsExausted => fringe.Count == 0;

        public SteppableTopologyGenerator(
            Topology[,] topology,
            PathType[,] traversable,
            Vector2Int dimensions,
            IEnumerable<Vector2Int> initialFringe)
        {
            this.fringe = new List<Vector2Int>(initialFringe);
            this.fringeQueue = new List<Vector2Int>(40);
            this.ignoreSet = new HashSet<Vector2Int>();
            this.seenSet = new HashSet<Vector2Int>();
            this.currentHeight = 0;

            this.dimensions = dimensions;
            this.topology = topology;
            this.traversable = traversable;
        }

        public bool CalculateNextIteration()
        {
            if (fringe.Count == 0)
                return false;

            IEnumerable<(Vector2Int cell, Vector2 direction)> GetSteps(Vector2Int cell)
            {
                bool IsTraversable(Vector2Int c) =>
                        c.x >= 0 && c.x < dimensions.x &&
                        c.y >= 0 && c.y < dimensions.y &&
                        traversable[c.x, c.y] == PathType.TRAVERSABLE;

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

            // Update the height of all cells in the current fringe
            foreach (var cell in fringe)
            {
                var currentTopology = topology[cell.x, cell.y];
                topology[cell.x, cell.y].slope = Vector2.ClampMagnitude(currentTopology.slope, 1f);
                topology[cell.x, cell.y].height = currentHeight;
            }

            // Walk through each neighbor, updating the topology
            foreach (var cell in fringe)
            {
                foreach (var step in GetSteps(cell))
                {
                    // Add the cell to the next fringe if we've never seen it before
                    if (!ignoreSet.Contains(step.cell) && !seenSet.Contains(step.cell))
                    {
                        fringeQueue.Add(step.cell);
                        seenSet.Add(step.cell);

                        // Also reset the cell, we're about to update it for the first time
                        topology[step.cell.x, step.cell.y].slope = Vector2.zero;
                    }

                    if (topology[step.cell.x, step.cell.y].height > currentHeight)
                        topology[step.cell.x, step.cell.y].slope += step.direction;
                }
            }

            // Swap the fringe buffers
            var tmp = fringe;
            fringe = fringeQueue;

            fringeQueue = tmp;
            fringeQueue.Clear();

            currentHeight += 1;

            return true;
        }

        public void Handoff(SteppableTopologyGenerator generator)
        {
            generator.fringe.Clear();
            generator.fringe.AddRange(fringe);

            generator.fringeQueue.Clear();
            generator.seenSet.Clear();

            generator.ignoreSet.Clear();
            foreach (var cell in ignoreSet)
                generator.ignoreSet.Add(cell);
            foreach (var cell in seenSet)
                generator.ignoreSet.Add(cell);

            generator.currentHeight = currentHeight;
            generator.dimensions = dimensions;
            generator.topology = topology;
            generator.traversable = traversable;
        }

        public void Reset(IEnumerable<Vector2Int> initialFringe)
        {
            fringe.Clear();
            fringe.AddRange(initialFringe);

            fringeQueue.Clear();
            seenSet.Clear();
            currentHeight = 0;
        }
    }

    [SerializeField]
    [Min(1)]
    private int fastHeightCalculationBound = 20;

    [SerializeField]
    [Min(1)]
    private int slowHeightCalculationBound = 20;

    [SerializeField]
    private bool showDebug;

    private Vector2Int dimensions;
    private PathType[,] traversable;
    private Topology[,] topology;

    private IEnumerable<Vector2Int> targets;

    public Vector2Int Dimensions => dimensions;
    public bool IsInitialized => topology != null;

    public void InitalizeTopology(Vector2Int dimensions)
    {
        int width = dimensions.x;
        int height = dimensions.y;
        int nCells = width * height;

        this.dimensions = dimensions;
        this.traversable = new PathType[width, height];
        this.topology = new Topology[width, height];
        this.targets = new List<Vector2Int>();

        StopAllCoroutines();
        StartCoroutine(ContinuouslyRecalculateTopology());
        StartCoroutine(ShowDebug());
    }    

    public void SetTargets(IEnumerable<Vector2Int> targets)
    {
        this.targets = targets;
    }

    public void SetState(Vector2Int position, PathType state)
    {
        this.traversable[position.x, position.y] = state;
    }

    public Topology GetTopology(Vector2Int cell)
    {
        if (cell.x < 0 || cell.y < 0 || cell.x >= dimensions.x || cell.y >= dimensions.y)
            return new Topology { slope = Vector2Int.zero, height = 99999999 };

        return topology[cell.x, cell.y];
    }

    private IEnumerator ContinuouslyRecalculateTopology()
    {
        SteppableTopologyGenerator fastGenerator = new SteppableTopologyGenerator(
            topology, traversable, Dimensions, Enumerable.Empty<Vector2Int>());

        SteppableTopologyGenerator slowGenerator = new SteppableTopologyGenerator(
            topology, traversable, Dimensions, Enumerable.Empty<Vector2Int>());

        while (true)
        {
            // Reset the fast generator for this iteration
            fastGenerator.Reset(targets);

            // Compute the topology near the targets
            for (int i = 0; i < fastHeightCalculationBound; i++)
                fastGenerator.CalculateNextIteration();

            // If the slow generator is finished it's last job,
            // handoff the current fast generator state to it
            if (slowGenerator.IsExausted)
                fastGenerator.Handoff(slowGenerator);

            // Do some work on the slow generator
            for (int i = 0; i < slowHeightCalculationBound; i++)
                slowGenerator.CalculateNextIteration();

            yield return null;
        }
    }

    private IEnumerator ShowDebug()
    {
        while (true)
        {
            if (showDebug)
            {
                for (int x = 0; x < Dimensions.x; x++)
                {
                    for (int y = 0; y < Dimensions.y; y++)
                    {
                        var cellCenter = new Vector2(x, y) + new Vector2(0.5f, 0.5f);
                        var topology = GetTopology(new Vector2Int(x, y)).ClampedSlope;
                        Debug.DrawRay(cellCenter, topology * 0.4f, Color.red, 0.01f);
                    }
                }
            }

            yield return new WaitForSeconds(0.01f);
        }
    }
}
