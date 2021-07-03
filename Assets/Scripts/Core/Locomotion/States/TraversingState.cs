﻿using System;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using UnityEngine;

public class TraversingState : State, IPropertiesMutable<TraversingState.Properties>
{
    private static float EFFECTIVE_COLLISION_DISTANCE = 0.1f;

    [System.Serializable]
    public struct Properties
    {
        public float climbSpeed;
        public bool automaticDetachEnabled;
        public float maxCornerCutTriggerDistance;
    }

    private enum Direction
    {
        UP, DOWN, LEFT, RIGHT
    }

    public override string Name => "TraversingState";

    private SpideringInput input;
    private KinematicBody body;
    private Properties properties;

    public Event<Vector2> OnWallDetach;

    public TraversingState(SpideringInput input, KinematicBody body, Properties properties)
    {
        this.input = input;
        this.body = body;
        this.properties = properties;
    }

    public override void OnUpdate(IStateMachine sm)
    {
        if (IsAttemptingToDetatchFromWall() && CanJumpInDirection(input.MovementDirection))
        {
            OnWallDetach?.Invoke(input.MovementDirection);
            return;
        }

        if (input.MovementDirection.sqrMagnitude < 0.1f)
            return;

        if (ShouldCutCorner(input.MovementDirection))
        {
            OnWallDetach?.Invoke(input.MovementDirection);
            return;
        }

        if (IsBodyLatched(Direction.LEFT) || IsBodyLatched(Direction.RIGHT))
            MaybeMoveInDirection(mask: new Vector2(0, 1));

        if (IsBodyLatched(Direction.UP) || IsBodyLatched(Direction.DOWN))
            MaybeMoveInDirection(mask: new Vector2(1, 0));
    }

    private bool IsAttemptingToDetatchFromWall()
    {
        if (input.IsJumping)
            return true;

        if (!properties.automaticDetachEnabled)
            return false;

        if (IsBodyLatched(Direction.DOWN) && input.MovementDirection.y > 0.5f)
            return true;
        else if (IsBodyLatched(Direction.UP) && input.MovementDirection.y < -0.5f)
            return true;
        else if (IsBodyLatched(Direction.LEFT) && input.MovementDirection.x > 0.5f)
            return true;
        else if (IsBodyLatched(Direction.RIGHT) && input.MovementDirection.x < -0.5f)
            return true;

        return false;
    }

    private bool ShouldCutCorner(Vector2 dir)
    {
        if (properties.automaticDetachEnabled)
            return false;

        var heading = dir.normalized;
        var skinOffset = heading * body.SkinWidth * 4;
        
        bool canSeeWall = body.EffectiveBounds.GetCorners()
            .Any(corner => TestForCollision(corner + skinOffset, heading, properties.maxCornerCutTriggerDistance));

        bool canDetachFromWall = body.EffectiveBounds.GetCorners()
            .All(corner => !TestForCollision(corner + skinOffset, heading, EFFECTIVE_COLLISION_DISTANCE));

        return canSeeWall && canDetachFromWall;
    }

    private bool TestForCollision(Vector2 origin, Vector2 dir, float distance)
        => Physics2D.Raycast(origin, dir, distance, body.CollisionMask);

    private bool CanJumpInDirection(Vector2 dir)
    {
        foreach (var corner in body.EffectiveBounds.GetCorners())
        {
            var origin = corner + (dir.normalized * body.SkinWidth * 4);
            if (Physics2D.Raycast(origin, dir, EFFECTIVE_COLLISION_DISTANCE * 2f, body.CollisionMask))
                return false;
        }

        return true;
    }

    private bool MaybeMoveInDirection(Vector2 mask)
    {
        var targetDirection = input.MovementDirection * mask;
        if (targetDirection.sqrMagnitude < 0.001f)
            return false;

        var deltaPosition = targetDirection.normalized * Time.deltaTime * properties.climbSpeed;
        body.MoveAndCollide(deltaPosition);
        return deltaPosition.sqrMagnitude > 0f;
    }

    private bool IsBodyLatched(Direction direction)
    {
        return GetRays(direction, body.EffectiveBounds)
            .All(ray => Physics2D.Raycast(ray.origin, ray.direction, EFFECTIVE_COLLISION_DISTANCE, body.CollisionMask));
    }

    private IEnumerable<Ray2D> GetRays(Direction direction, Bounds bounds)
    {
        switch (direction)
        {
            case Direction.UP:
                yield return new Ray2D
                {
                    direction = Vector2.up,
                    origin = bounds.GetTopLeft()
                };

                yield return new Ray2D
                {
                    direction = Vector2.up,
                    origin = bounds.GetTopRight()
                };

                break;

            case Direction.DOWN:
                yield return new Ray2D
                {
                    direction = Vector2.down,
                    origin = bounds.GetBottomLeft()
                };

                yield return new Ray2D
                {
                    direction = Vector2.down,
                    origin = bounds.GetBottomRight()
                };

                break;

            case Direction.RIGHT:
                yield return new Ray2D
                {
                    direction = Vector2.right,
                    origin = bounds.GetTopRight()
                };

                yield return new Ray2D
                {
                    direction = Vector2.right,
                    origin = bounds.GetBottomRight()
                };

                break;

            case Direction.LEFT:
                yield return new Ray2D
                {
                    direction = Vector2.left,
                    origin = bounds.GetBottomLeft()
                };

                yield return new Ray2D
                {
                    direction = Vector2.left,
                    origin = bounds.GetTopLeft()
                };

                break;
        }
    }

    public void UpdateProperties(PropertiesUpdating<Properties> updater)
        => updater?.Invoke(ref properties);
}
