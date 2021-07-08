using UnityEngine;
using System.Collections;

public class CameraProjection : MonoBehaviour
{
    private static CameraProjection instance;

    [SerializeField]
    private Camera mainCamera;

    public Rect WorldBounds
    {
        get
        {
            var rectMin = ViewportToWorld(Vector2.zero);
            var rectMax = ViewportToWorld(Vector2.one);
            return new Rect(rectMin, rectMax - rectMin);
        }
    }

    public Vector2 WorldToViewport(Vector2 position)
        => mainCamera.WorldToViewportPoint(position);

    public Vector2 ViewportToWorld(Vector2 position)
        => mainCamera.ViewportToWorldPoint(new Vector3(position.x, position.y, mainCamera.nearClipPlane));

    private void Awake()
    {
        instance = this;
    }

    private void OnDestroy()
    {
        instance = null;
    }

    private void OnDrawGizmos()
    {
        if (mainCamera == null)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawCube(WorldBounds.min, Vector3.one * 0.1f);

        Gizmos.color = Color.cyan;
        Gizmos.DrawCube(WorldBounds.max, Vector3.one * 0.1f);
    }

    public static bool TryGetCurrent(out CameraProjection projection)
    {
        projection = instance;
        return projection != null;
    }
}
