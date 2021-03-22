using UnityEngine;
using System.Collections;

[RequireComponent(typeof(LineRenderer))]
public class PlanetRegionEdge : MonoBehaviour
{
    private LineRenderer lineRenderer;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    public void SetEdge(Edge edge)
    {
        lineRenderer.positionCount = 2;
        lineRenderer.SetPositions(new Vector3[] {
            edge.p1,
            edge.p2
        });
    }
}
