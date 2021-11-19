using System;
using UnityEngine;

public interface ITraversableGrid
{
    Vector2Int Dimensions { get; }
    bool IsTraversable(Vector2Int position);
}
