using UnityEngine;
using System.Collections;

public class RectGizmo : MonoBehaviour
{
    [SerializeField]
    private Color color;

    [SerializeField]
    private Vector3 size;

    private void OnDrawGizmos()
    {
        Gizmos.color = color;
        Gizmos.DrawCube(transform.position, size);
    }
}
