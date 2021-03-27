using UnityEngine;
using System.Collections;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(EdgeCollider2D))]
[RequireComponent(typeof(CollisionReceiver))]
public class PlanetRegionEdge : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private EdgeCollider2D edgeCollider;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        edgeCollider = GetComponent<EdgeCollider2D>();
        GetComponent<CollisionReceiver>().OnCollisionTrigger += OnCollisionTriggered;
    }

    public void SetEdge(Edge edge)
    {
        lineRenderer.positionCount = 2;
        lineRenderer.SetPositions(new Vector3[] {
            edge.p1,
            edge.p2
        });

        edgeCollider.points = new Vector2[] {
            edge.p1,
            edge.p2
        };
    }

    private void OnCollisionTriggered(CollisionTrigger trigger)
    {
        Debug.Log("Collision wth region edge");
    }
}
