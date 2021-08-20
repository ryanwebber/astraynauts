using UnityEngine;
using System.Collections;

public class Heading2D : MonoBehaviour
{
    [SerializeField]
    [ReadOnly]
    private Vector2 heading;
    public Vector2 CurrentHeading => heading;

    private Vector2 previousPosition;

    private void Start()
    {
        previousPosition = transform.position;
    }

    private void LateUpdate()
    {
        var deltaPosition = ((Vector2)transform.position - previousPosition);
        if (deltaPosition.magnitude < 0.0001f)
            heading = Vector2.zero;
        else
            heading = deltaPosition.normalized;

        previousPosition = transform.position;
    }
}
