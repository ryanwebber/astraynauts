using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LocomotableInput))]
public class LocomotableActor : MonoBehaviour, IActivatable
{
    private LocomotableInput virtualInput;

    public bool IsActive { get; set; }

    private void Awake()
    {
        virtualInput = GetComponent<LocomotableInput>();
    }
}
