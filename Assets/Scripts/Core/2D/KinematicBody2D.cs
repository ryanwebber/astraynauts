using UnityEngine;
using System.Collections;
using System;
using Extensions;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class KinematicBody2D : MonoBehaviour
{
    const float kMinMovementValue = 0.001f;

    [System.Serializable]
    public struct Collision
    {
        public static Collision Empty = new Collision();

        public RaycastHit2D above;
        public RaycastHit2D below;
        public RaycastHit2D left;
        public RaycastHit2D right;
    }

    [SerializeField]
    private LayerMask collisionMask;

    [SerializeField]
    private float skinWidth = 0.02f;

    [SerializeField]
    private int horizontalRays = 4;

    [SerializeField]
    private int verticalRays = 4;

    private BoxCollider2D boxCollider;
    private Collision collisionState;

    private Bounds EffectiveBounds
    {
        get
        {
            var modifiedBounds = boxCollider.bounds;
            modifiedBounds.Expand(-2f * skinWidth);
            return modifiedBounds;
        }
    }

    private float VerticalRaySpacing
    {
        get
        {
            var colliderUseableHeight = boxCollider.size.y * Mathf.Abs(transform.localScale.y) - (2f * skinWidth);
            return colliderUseableHeight / (horizontalRays - 1);
        }
    }

    private float HorizontalRaySpacing
    {
        get
        {
            var colliderUseableWidth = boxCollider.size.x * Mathf.Abs(transform.localScale.x) - (2f * skinWidth);
            return colliderUseableWidth / (verticalRays - 1);
        }
    }

    public LayerMask CollisionMask
    {
        get => collisionMask;
        set => collisionMask = value;
    }

    public Collision CollisionState
    {
        get => collisionState;
    }

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
    }

    public Vector2 MoveAndCollide(Vector2 translation)
    {
        Collision _;
        return MoveAndCollide(translation, out _);
    }

    public Vector2 MoveAndCollide(Vector2 translation, out Collision collision)
    {
        ResetCollisionState();

        if (Mathf.Abs(translation.x) > kMinMovementValue)
            MoveHorizontal(ref translation, ref collisionState);

        if (Mathf.Abs(translation.y) > kMinMovementValue)
            MoveVertical(ref translation, ref collisionState);

        if (translation.magnitude > kMinMovementValue)
            transform.position += (Vector3)translation;

        collision = collisionState;
        return translation;
    }

    public void ResetCollisionState()
    {
        collisionState = default;
    }

    private void MoveHorizontal(ref Vector2 translation, ref Collision collisionData)
    {
        Vector2 originalTranslation = translation;

        var isGoingRight = translation.x > 0;
        var rayDistance = Mathf.Abs(translation.x) + skinWidth;
        var rayDirection = isGoingRight ? Vector2.right : Vector2.left;
        var initialRayOrigin = isGoingRight ? EffectiveBounds.GetBottomRight() : EffectiveBounds.GetBottomLeft();

        RaycastHit2D _raycastHit = default;
        float minHitDistance = float.PositiveInfinity;

        for (var i = 0; i < horizontalRays; i++)
        {
            var origin = new Vector2(initialRayOrigin.x, initialRayOrigin.y + i * VerticalRaySpacing);

            Debug.DrawRay(origin, rayDirection * rayDistance, Color.red);
            _raycastHit = Physics2D.Raycast(origin, rayDirection, rayDistance, collisionMask);

            if (_raycastHit && _raycastHit.distance < minHitDistance)
            {
                // Keep track of our new nearest collision
                minHitDistance = _raycastHit.distance;

                // Reset back to the original translation before making the correction
                translation = originalTranslation;

                // Set the new deltaMovement and recalculate the rayDistance taking it into account
                translation.x = _raycastHit.point.x - origin.x;

                // Update the collision data
                if (isGoingRight)
                {
                    translation.x -= skinWidth;
                    collisionData.right = _raycastHit;
                }
                else
                {
                    translation.x += skinWidth;
                    collisionData.left = _raycastHit;
                }
            }
        }
    }

    private void MoveVertical(ref Vector2 translation, ref Collision collisionData)
    {
        Vector2 originalTranslation = translation;

        var isGoingUp = translation.y > 0;
        var rayDistance = Mathf.Abs(translation.y) + skinWidth;
        var rayDirection = isGoingUp ? Vector2.up : Vector2.down;
        var initialRayOrigin = isGoingUp ? EffectiveBounds.GetTopLeft() : EffectiveBounds.GetBottomLeft();

        // apply our horizontal deltaMovement here so that we do our raycast from the actual position we would be in if we had moved
        initialRayOrigin.x += translation.x;

        RaycastHit2D _raycastHit = default;
        float minHitDistance = float.PositiveInfinity;

        for (var i = 0; i < verticalRays; i++)
        {
            var origin = new Vector2(initialRayOrigin.x + i * HorizontalRaySpacing, initialRayOrigin.y);

            Debug.DrawRay(origin, rayDirection * rayDistance, Color.red);
            _raycastHit = Physics2D.Raycast(origin, rayDirection, rayDistance, collisionMask);

            if (_raycastHit && _raycastHit.distance < minHitDistance)
            {
                // Keep track of our new nearest collision
                minHitDistance = _raycastHit.distance;

                // Reset back to the original translation before making the correction
                translation = originalTranslation;

                // Set our new deltaMovement and recalculate the rayDistance taking it into account
                translation.y = _raycastHit.point.y - origin.y;

                if (isGoingUp)
                {
                    translation.y -= skinWidth;
                    collisionData.above = _raycastHit;
                }
                else
                {
                    translation.y += skinWidth;
                    collisionData.below = _raycastHit;
                }
            }
        }
    }
}
