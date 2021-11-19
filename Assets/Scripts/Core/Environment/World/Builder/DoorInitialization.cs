using System;
using UnityEngine;

public struct DoorInitialization : IOperation
{
    public Door door;
    public DoorController prefab;
    public Vector2 offset;

    public void Perform()
    {
        var doorRef = this.door;
        var instance = UnityEngine.Object.Instantiate(prefab, doorRef.Center + offset, Quaternion.identity);
        instance.ReferencedDoor = doorRef;
        instance.OnDoorStateChanged += state =>
        {
            doorRef.Update(isOpen: state.isOpen);
        };
    }
}
