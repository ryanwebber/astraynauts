using UnityEngine;
using System.Collections;

[RequireComponent(typeof(RaycastBody))]
public class TestRaycaster : MonoBehaviour
{
    [SerializeField]
    private Vector2 velocity;

    [SerializeField]
    private Transform sprite;

    [SerializeField]
    private Vector2 collisionSmush;

    [SerializeField]
    private float animationDuration = 0.2f;

    [SerializeField]
    private bool moving = true;

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

        velocity = raycastBody.MoveAndBounce(velocity) * normalizedVelocity;
        sprite.transform.rotation = Quaternion.LookRotation(Vector3.back, velocity);

        if (raycastBody.CollisionCount > 0)
        {
            StopAllCoroutines();
            transform.localScale = collisionSmush;
            TweenBuilder.WaitSeconds(0f)
                .ThenScale(transform, Vector3.one, animationDuration)
                .Start(this);
        }
    }
}
