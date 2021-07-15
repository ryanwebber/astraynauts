using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ProjectileSpawner : MonoBehaviour
{
    public Event<Projectile> Decorators;
    public Event<Projectile> OnProjectileSpawn;

    [SerializeField]
    private Projectile prefab;

    [SerializeField]
    private Transform spawnOrigin;

    public void SpawnProjectile(Vector2 direction)
    {
        if (direction.sqrMagnitude < 0.001f || prefab == null)
            return;

        var instance = Instantiate(prefab, position: spawnOrigin.position, rotation: Quaternion.identity);
        instance.OnProjectileSpawn?.Invoke(direction.normalized);

        Decorators?.Invoke(instance);
        OnProjectileSpawn?.Invoke(instance);
    }
}
