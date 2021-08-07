using UnityEngine;
using System.Collections;

[RequireComponent(typeof(HealthManager))]
public class DamageReceiver : MonoBehaviour
{
    private HealthManager healthManager;

    private void Awake()
    {
        healthManager = GetComponent<HealthManager>();
    }

    public void DealDamage(int damage)
    {
        healthManager?.AddHealth(-Mathf.Abs(damage));
    }
}
