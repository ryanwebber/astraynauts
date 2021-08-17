using UnityEngine;
using System.Collections;

public class DoorDescriptor : WorldGrid.Descriptor
{
    public readonly Door Door;

    public DoorDescriptor(Door door)
    {
        Door = door;
    }
}
