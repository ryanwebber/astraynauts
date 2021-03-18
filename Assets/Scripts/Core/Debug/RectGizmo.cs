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
        Matrix4x4 oldGizmosMatrix = Gizmos.matrix;

        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = color;
 
        Gizmos.DrawCube(Vector3.zero, size);

        Gizmos.matrix = oldGizmosMatrix;
    }
}
