using System;
using UnityEngine;

public class DoorInitialization : IOperation
{
    private Door door;
    private DoorController prefab;

    public DoorInitialization(Door door, DoorController prefab)
    {
        this.door = door;
        this.prefab = prefab;
    }

    public void Perform()
    {
        // TODO: Don't do this, door should slot in based on pixel coordinates
        var tempOffset = Vector2.up * 0.39061f;

        Debug.Log("Adding offset to spawned door: " + tempOffset);

        var instance = UnityEngine.Object.Instantiate(prefab, door.Center + tempOffset, Quaternion.identity);
        instance.OnDoorStateChanged += state => door.Update(isOpen: state.isOpen);
    }
}
