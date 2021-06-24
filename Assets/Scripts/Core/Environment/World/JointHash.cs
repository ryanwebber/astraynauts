using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public struct JointHash
{
    [System.Serializable]
    public enum Direction
    {
        UP = 1, RIGHT = 2, DOWN = 3, LEFT = 4
    }

    public int hash;

    public JointHash(int hash)
    {
        this.hash = hash;
    }

    public JointHash(IEnumerable<Direction> directions)
    {
        hash = 0;
        foreach (var dir in directions)
            hash |= 1 << (int)dir;
    }

    public JointHash(params Direction[] directions) : this((IEnumerable<Direction>)directions)
    {
    }

    public bool Contains(Direction direction)
    {
        return (hash & (int)direction) != 0;
    }

    public IEnumerable<Direction> GetDirections()
    {
        var retained = this;
        return EnumUtil.GetValues<Direction>().Where(dir => retained.Contains(dir));
    }

    public static JointHash operator +(JointHash jointHash, Direction direction)
    {
        return new JointHash(jointHash.hash | (1 << (int)direction));
    }


    public static Vector2Int DirectionVector(Direction direction)
    {
        switch (direction)
        {
            case Direction.UP:
                return Vector2Int.up;
            case Direction.RIGHT:
                return Vector2Int.right;
            case Direction.DOWN:
                return Vector2Int.down;
            case Direction.LEFT:
                return Vector2Int.left;
        }

        return Vector2Int.zero;
    }

    public static IEnumerable<Direction> Directions => EnumUtil.GetValues<Direction>();

    [System.Serializable]
    public struct JointDefinition
    {
        [SerializeField]
        private List<Direction> openEnds;

        public JointHash Hash => new JointHash(openEnds);
    }
}
