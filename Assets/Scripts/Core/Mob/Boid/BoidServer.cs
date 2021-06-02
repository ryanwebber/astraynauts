using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Rendering;

public class BoidServer : MonoBehaviour
{
    public struct Perception
    {
        public Vector2 cumulativeFlockCenter;
        public Vector2 cumulativeFlockHeading;
        public Vector2 cumulativeLocalRepultion;

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

    private struct BoidData
    {
        public Vector2 currentPosition;
        public Vector2 currentHeading;

        public float perceptionRadius;
        public float avoidanceRadius;

        public int computedFlockCount;
        public Vector2 computedFlockCenter;
        public Vector2 computedFlockHeading;
        public Vector2 computedLocalRepultion;

        private static int Vect2Size => sizeof(float) * 2;
        private static int FloatSize => sizeof(float);
        private static int IntSize => sizeof(int);
        public static readonly int Size = (Vect2Size * 5) + (FloatSize * 2) + (IntSize * 1);
    }
    private static BoidServer _instance;
    public static BoidServer Instance => _instance;

    [SerializeField]
    private Vector2Int gridSize;

    [SerializeField]
    private Vector2 cellSize;

    public Rect Bounds => new Rect(Vector2.zero, cellSize * gridSize);

    private Dictionary<Boid, Perception> boids;
    private SpaciallyPartitionedCollection<Boid, Vector2> partition;

    private void Awake()
    {
        if (_instance != null)
            throw new System.Exception("Boid server already exists");

        _instance = this;

        boids = new Dictionary<Boid, Perception>();

        var hashFn = SpaciallyPartitionedCollection.CreateGridHash(cellSize, gridSize, out var nBuckets);
        partition = new SpaciallyPartitionedCollection<Boid, Vector2>(nBuckets, hashFn);
    }

    private void Update()
    {
        for (int i = boids.Count - 1; i >= 0; i--)
        {
            var boid = boids.ElementAt(i).Key;
            if (boid == null)
            {
                boids.Remove(boid);
                partition.Remove(boid);
                continue;
            }
        }

        foreach (var boid in boids.Keys)
            partition.AddOrUpdate(boid, boid.transform.position);

        foreach (var boid in partition)
            UpdatePerception(boid);
    }

    private bool IsInBounds(Vector2 pos)
    {
        return pos.x >= 0 && pos.x < (gridSize.x * cellSize.x) &&
            pos.y >= 0 && pos.y < (gridSize.y * cellSize.y);
    }

    private void UpdatePerception(Boid boid)
    {
        var localBoids = CELL_BLOCK
            .Select(dir => (Vector2)boid.transform.position + dir * cellSize)
            .Where(IsInBounds)
            .SelectMany(pos => partition.GetBucket(pos));
            //.ToArray();

        var perception = new Perception
        {
            cumulativeFlockCenter = Vector2.zero,
            cumulativeFlockHeading = Vector2.zero,
            cumulativeLocalRepultion = Vector2.zero,
            flockCount = 0
        };

        //Debug.Log($"Num local boids: {localBoids.Length}");

        foreach (var otherBoid in localBoids.Where(b => b != this))
        {
            Vector2 relPos = otherBoid.CurrentPosition - boid.CurrentPosition;
            float sqrDst = relPos.SqrMagnitude();

            if (sqrDst < boid.Params.SqrViewRadius)
            {
                perception.flockCount++;

                perception.cumulativeFlockHeading += otherBoid.CurrentHeading;
                perception.cumulativeFlockCenter += otherBoid.CurrentPosition;

                if (sqrDst < boid.Params.SqrAvoidanceRadius && sqrDst != 0f)
                {
                    perception.cumulativeLocalRepultion -= relPos / sqrDst;
                }
            }
        }

        boids[boid] = perception;
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
        boids.Add(boid, default);
        partition.AddOrUpdate(boid, boid.CurrentPosition);
    }

    public void Unregister(Boid boid)
    {
        boids.Remove(boid);
        partition.Remove(boid);
    }

    public Perception GetPerception(Boid boid)
    {
        if (boids.ContainsKey(boid))
            return boids[boid];

        Debug.LogWarning("Boid is not a member of the boid server");
        return default;
    }
}
