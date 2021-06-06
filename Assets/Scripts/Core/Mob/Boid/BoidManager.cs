using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class BoidManager : MonoBehaviour
{
    public struct LocalPerception
    {
        public Vector2 cumulativeFlockCenter;
        public Vector2 cumulativeFlockHeading;
        public int flockCount;

        public Vector2 FlockCenter => flockCount == 0 ? cumulativeFlockCenter : cumulativeFlockCenter / flockCount;
    }

    private static readonly Vector2Int[] CELL_BLOCK = new Vector2Int[]
    {
        Vector2Int.zero,

        Vector2Int.up,
        Vector2Int.right,
        Vector2Int.down,
        Vector2Int.left,

        new Vector2Int(1, 1),
        new Vector2Int(1, -1),
        new Vector2Int(-1, 1),
        new Vector2Int(-1, -1),
    };

    private static BoidManager _instance;
    public static BoidManager Instance => _instance;

    [SerializeField]
    private Vector2Int gridSize;

    [SerializeField]
    private Vector2 cellSize;

    public Rect Bounds => new Rect(Vector2.zero, cellSize * gridSize);

    private HashSet<Boid> allBoids;
    private Dictionary<Vector2Int, LocalPerception> cellularPerceptions;
    private SpaciallyPartitionedCollection<Boid, Vector2Int> spatialPartition;

    private void Awake()
    {
        if (_instance != null)
            throw new System.Exception("Boid server already exists");

        _instance = this;


        int nBuckets = gridSize.x * gridSize.y;
        System.Func<Vector2Int, int> hashFn = cell => cell.x + (cell.y * gridSize.x);
        spatialPartition = new SpaciallyPartitionedCollection<Boid, Vector2Int>(nBuckets, hashFn);

        allBoids = new HashSet<Boid>();
        cellularPerceptions = new Dictionary<Vector2Int, LocalPerception>();
    }

    private void Update()
    {
        cellularPerceptions.Clear();
        for (int i = allBoids.Count - 1; i >= 0; i--)
        {
            var boid = allBoids.ElementAt(i);
            if (boid == null)
            {
                allBoids.Remove(boid); // This will be a problem
                spatialPartition.Remove(boid);
                continue;
            }
        }

        foreach (var boid in allBoids)
            spatialPartition.AddOrUpdate(boid, GetCell(boid.CurrentPosition));

        foreach (var boid in allBoids)
            UpdateCellularPerception(boid);
    }

    private Vector2Int GetCell(Vector2 position)
    {
        return new Vector2Int(
            Mathf.FloorToInt(position.x / cellSize.x),
            Mathf.FloorToInt(position.y / cellSize.y)
        );
    }

    private bool IsInBounds(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < gridSize.x &&
            pos.y >= 0 && pos.y < gridSize.y;
    }

    private void UpdateCellularPerception(Boid boid)
    {
        var currentCell = GetCell(boid.CurrentPosition);
        if (cellularPerceptions.TryGetValue(currentCell, out var perception))
            return;

        var numCells = CELL_BLOCK
            .Select(dir => currentCell + dir)
            .Where(IsInBounds)
            .Count();

        var localBoids = CELL_BLOCK
            .Select(dir => currentCell + dir)
            .Where(IsInBounds)
            .SelectMany(cell => spatialPartition.GetBucket(cell));

        Vector2 cumulativeFlockCenter = Vector2.zero;
        Vector2 cumulativeFlockHeading = Vector2.zero;
        int flockCount = 0;

        foreach (var itrBoid in localBoids)
        {
            flockCount++;
            cumulativeFlockHeading += itrBoid.CurrentHeading;
            cumulativeFlockCenter += itrBoid.CurrentPosition;
        }

        cellularPerceptions[currentCell] = new LocalPerception
        {
            cumulativeFlockCenter = cumulativeFlockCenter,
            cumulativeFlockHeading = cumulativeFlockHeading,
            flockCount = flockCount,
        };
    }

    private void OnDestroy()
    {
        _instance = null;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(Bounds.center, new Vector3(Bounds.width, Bounds.height, 1f));
    }

    public void Register(Boid boid)
    {
        allBoids.Add(boid);
        spatialPartition.AddOrUpdate(boid, GetCell(boid.CurrentPosition));
    }

    public void Unregister(Boid boid)
    {
        allBoids.Remove(boid);
        spatialPartition.Remove(boid);
    }

    public LocalPerception GetPerception(Boid boid)
    {
        var cell = GetCell(boid.CurrentPosition);
        if (cellularPerceptions.TryGetValue(cell, out var localPerception))
            return localPerception;

        return default;
    }
}
