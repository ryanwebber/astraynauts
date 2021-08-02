using System;
using UnityEngine;

public class Teleporter
{
    public readonly Vector2 Center;
    public readonly Vector2 OpenFace;
    public readonly Room AttachedRoom;

    public Teleporter(Vector2 center, Vector2 openFace, Room attachedRoom)
    {
        Center = center;
        OpenFace = openFace;
        AttachedRoom = attachedRoom;
    }
}
