using UnityEngine;
using System.Collections;


public class PlanetRegion : MonoBehaviour
{
    [SerializeField]
    private Pentagon shape;
    public Pentagon Bounds => shape;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        foreach (var edge in shape.GetEdges())
        {
            Gizmos.DrawLine(edge.p1, edge.p2);
        }

        Bounds bounds = shape.BoundingRect;
        Gizmos.DrawWireCube(bounds.center + transform.position, bounds.size);
    }
}
