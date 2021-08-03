using UnityEngine;
using System.Collections;

public class RectGizmo : MonoBehaviour
{
    [SerializeField]
    public Color color;

    [SerializeField]
    public Vector3 size;

    private void OnDrawGizmos()
    {
        Gizmos.color = color;
        Gizmos.DrawCube(transform.position, size);
    }
}
