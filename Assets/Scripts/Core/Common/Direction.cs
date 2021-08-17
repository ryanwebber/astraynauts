using System;
using System.Collections.Generic;
using UnityEngine;

public struct Direction : IEquatable<Direction>
{
    public enum Face
    {
        RIGHT, LEFT, UP, DOWN
    }

    private Face face;
    public Face Facing
    {
        get => face;
        set => face = value;
    }

    public Direction Reverse => new Direction(Heading * -1);

    public bool IsHorizontal => Heading.x != 0;
    public bool IsVertical => !IsHorizontal;

    public Vector2Int Heading
    {
        get
        {
            switch (face)
            {
                case Direction.Face.DOWN:
                    return Vector2Int.down;
                case Direction.Face.UP:
                    return Vector2Int.up;
                case Direction.Face.RIGHT:
                    return Vector2Int.right;
                case Direction.Face.LEFT:
                    return Vector2Int.left;
            }

            throw new Exception($"Unexpected faceing direction: {face}");
        }

        set
        {
            if (value == Vector2Int.down)
                face = Direction.Face.DOWN;
            else if (value == Vector2Int.up)
                face = Direction.Face.UP;
            else if (value == Vector2Int.right)
                face = Direction.Face.RIGHT;
            else if (value == Vector2Int.left)
                face = Direction.Face.LEFT;
        }
    }

    public Direction(Face face)
    {
        this.face = face;
    }

    public Direction(Vector2Int heading)
    {
        this.face = Direction.Face.RIGHT;
        this.Heading = heading;
    }

    public override bool Equals(object obj)
        => obj is Direction direction && Equals(direction);

    public bool Equals(Direction other)
        => face == other.face;

    public override int GetHashCode()
    {
        return 286146108 + face.GetHashCode();
    }

    public static implicit operator Vector2Int(Direction direction)
        => direction.Heading;

    public static implicit operator Vector2(Direction direction)
        => direction.Heading;

    public static implicit operator Vector3(Direction direction)
        => (Vector2)direction.Heading;

    public static bool operator ==(Direction left, Direction right)
        => left.Equals(right);

    public static bool operator !=(Direction left, Direction right)
        => !(left == right);

    public static Direction Left => new Direction(Face.LEFT);
    public static Direction Right => new Direction(Face.RIGHT);
    public static Direction Up => new Direction(Face.UP);
    public static Direction Down => new Direction(Face.DOWN);

    public static readonly IEnumerable<Direction> Orthogonals = new Direction[]
    {
        Up, Right, Down, Left
    };
}
