using System;
using UnityEngine;

public class Airlock
{
    public readonly Vector2 Center;
    public readonly Vector2 OpenFace;

    public Airlock(Vector2 center, Vector2 openFace)
    {
        Center = center;
        OpenFace = openFace;
    }
}
