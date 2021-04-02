using UnityEngine;
using System.Collections;


public class PlanetRegion : MonoBehaviour
{
    [SerializeField]
    private Pentagon shape;
    public Pentagon Bounds => shape;

    private PlanetRegionOrientation orientation;
    public PlanetRegionOrientation Orientation
    {
        get => orientation;
        set
        {
            orientation = value;
            shape.rotation = value == PlanetRegionOrientation.NorthFacing ? 0 : Mathf.PI;
        }
    }

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
