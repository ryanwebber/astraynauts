using UnityEngine;
using System.Collections;

public class Follow2D : MonoBehaviour
{
    [SerializeField]
    private Transform target;

    private void LateUpdate()
    {
        float xPos = target.position.x;
        float yPos = target.position.y;
        float zPos = transform.position.z;
        transform.position = new Vector3(xPos, yPos, zPos);
    }
}
