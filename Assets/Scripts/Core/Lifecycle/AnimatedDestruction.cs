using System;
using UnityEngine;

[RequireComponent(typeof(DestructionTrigger))]
public class AnimatedDestruction: MonoBehaviour
{
    private void Awake()
    {
        GetComponent<DestructionTrigger>().OnDestructionTriggered += () =>
        {
            Debug.Log("Destruction triggered: " + gameObject.name);
            Destroy(gameObject);
        };
    }
}
