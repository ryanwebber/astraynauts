using UnityEngine;
using System.Collections;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(EdgeCollider2D))]
[RequireComponent(typeof(CollisionReceiver))]
public class PlanetRegionEdge : MonoBehaviour
{
    public Event<PlanetRegionEdge> OnCollideWithEdge;

    private LineRenderer lineRenderer;
    private EdgeCollider2D edgeCollider;

    private int? index;
    public int Index
    {
        get
        {
            if (index is int idx)
                return idx;
            else
                throw new System.NullReferenceException("Region index for edge has not been assigned yet");
        }
    }

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        edgeCollider = GetComponent<EdgeCollider2D>();
        GetComponent<CollisionReceiver>().OnCollisionTrigger += OnCollisionTriggered;
    }

    public void SetEdge(Edge edge, int index)
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

        this.index = index;
    }

    private void OnCollisionTriggered(CollisionTrigger trigger)
    {
        OnCollideWithEdge?.Invoke(this);
    }
}
