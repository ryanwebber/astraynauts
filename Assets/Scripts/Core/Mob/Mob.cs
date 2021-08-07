using UnityEngine;
using System.Collections;

[RequireComponent(typeof(DestructionTrigger))]
public class Mob : MonoBehaviour
{
    public Event<float> OnWillSpawnIntoWorld;
    public Event OnDidSpawnIntoWorld;
    public Event OnMobDefeated;

    private void Awake()
    {
        GetComponent<DestructionTrigger>().OnDestructionTriggered += () =>
        {
            OnMobDefeated?.Invoke();
        };
    }
}
