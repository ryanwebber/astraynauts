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

    public DamageResult DealDamage(int damage)
    {
        return healthManager.DealDamage(damage);
    }
}
