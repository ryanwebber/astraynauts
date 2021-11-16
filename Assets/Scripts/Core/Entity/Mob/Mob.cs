using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(DestructionTrigger))]
public class Mob : MonoBehaviour
{
    public Event OnMobSpawn;
    public Event OnMobDefeated;

    private void Awake()
    {
        GetComponent<DestructionTrigger>().OnDestructionTriggered += () =>
        {
            OnMobDefeated?.Invoke();
        };
    }
}
