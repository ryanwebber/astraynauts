using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(HealthManager))]
[RequireComponent(typeof(DestructionTrigger))]
public class HealthDepletedDestruction : MonoBehaviour
{
    private void Awake()
    {
        var destructionTrigger = GetComponent<DestructionTrigger>();
        var healthManager = GetComponent<HealthManager>();

        healthManager.OnHealthValueChanged += delta =>
        {
            if (delta.newValue == 0)
            {
                destructionTrigger.DestroyWithBehaviour();
            }
        };
    }
}
