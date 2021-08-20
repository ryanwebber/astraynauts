using System;
using UnityEngine;

public class Teleporter: ComponentMarker
{
    public readonly Vector2 Center;
    public readonly Direction OpenFace;
    public readonly Room AttachedRoom;

    public Teleporter(Vector2 center, Direction openFace, Room attachedRoom)
    {
        Center = center;
        OpenFace = openFace;
        AttachedRoom = attachedRoom;
    }
}
