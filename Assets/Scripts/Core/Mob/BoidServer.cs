using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class BoidServer : MonoBehaviour
{
    private static Vector2Int[] CELL_BLOCK_OFFSETS = new Vector2Int[]
    {
        // Current cell
        Vector2Int.zero,

        // Neighboring cells
        Vector2Int.up,
        new Vector2Int(1, 1),
        Vector2Int.right,
        new Vector2Int(1, -1),
        Vector2Int.down,
        new Vector2Int(-1, -1),
        Vector2Int.left,
        new Vector2Int(-1, 1)
    };

    private static BoidServer _instance;
    public static BoidServer Instance => _instance;

    [SerializeField]
    private Vector2 sectorSize;

    [SerializeField]
    private Vector2Int numSectors;

    private Vector2 Center => ((Vector2)transform.position) + Size / 2f;
    private Vector2 Size => sectorSize * numSectors;

    public Rect Bounds => new Rect(transform.position, Size);

    private SpaciallyPartitionedCollection<Boid> partition;
    private List<Boid> boids;

    private void Awake()
    {
        if (_instance != null)
            throw new System.Exception("Boid server already exists");

        _instance = this;

        partition = new SpaciallyPartitionedCollection<Boid>(numSectors, sectorSize);
        boids = new List<Boid>();
    }

    private void OnDestroy()
    {
        _instance = null;
    }

    private void Update()
    {
        foreach (var boid in boids)
        {
            partition[boid] = boid.CurrentPosition;
        }
    }

    public void Register(Boid boid)
    {
        partition[boid] = boid.CurrentPosition;
        boids.Add(boid);
    }

    public void Unregister(Boid boid)
    {
        boids.Remove(boid);
        partition.Remove(boid);
    }

    public IEnumerable<Boid> GetFlock(Vector2 position)
    {
        return CELL_BLOCK_OFFSETS
            .SelectMany(d =>
            {
                var cell = partition.PositionToCell(position) + d;
                return partition.GetEntitiesInCell(cell);
            })
            .Select(e => e.entity);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(Center, Size);
    }
}
