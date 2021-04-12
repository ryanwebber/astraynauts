using UnityEngine;
using System.Collections;

[RequireComponent(typeof(RaycastBody))]
public class TestRaycaster : MonoBehaviour
{

    [SerializeField]
    private Vector2 velocity;

    [SerializeField]
    private bool moving = false;

    private float normalizedVelocity;

    private RaycastBody raycastBody;

    private void Awake()
    {
        raycastBody = GetComponent<RaycastBody>();
        normalizedVelocity = velocity.magnitude;
    }

    void Update()
    {
        if (!moving)
            return;

        Debug.DrawRay(transform.position, velocity, Color.white);
        velocity = raycastBody.MoveAndBounce(velocity) * normalizedVelocity;
    }
}
