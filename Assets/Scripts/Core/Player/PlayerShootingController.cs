using UnityEngine;

[RequireComponent(typeof(ProjectileSpawner))]
[RequireComponent(typeof(PlayerInputFeedback))]
public class PlayerShootingController : MonoBehaviour
{
    [System.Serializable]
    public struct Properties
    {
        [SerializeField]
        public float burstFireDelay;

        [SerializeField]
        public float burstNotchChargeTime;

        [SerializeField]
        public int maxBurstCount;
    }

    [SerializeField]
    private Properties properties;

    public Vector2 AimValue;
    public Event OnFireBegin;
    public Event OnFireEnd;

    private ProjectileSpawner projectileSpawner;
    private PlayerInputFeedback inputFeedback;

    private void Awake()
    {
        inputFeedback = GetComponent<PlayerInputFeedback>();
        projectileSpawner = GetComponent<ProjectileSpawner>();

        OnFireBegin += () => Debug.Log("Charging...");
        OnFireEnd += () =>
        {
            Debug.Log("Fire!");
            FireProjectile();
        };

        projectileSpawner.Decorators += projectile =>
        {
            // TODO: Player-based decoration
        };
    }

    private void FireProjectile()
    {
        inputFeedback.TriggerHapticInstant();
        projectileSpawner.SpawnProjectile(AimValue);
    }

    private void OnDrawGizmos()
    {
        if (AimValue.sqrMagnitude > 0.001f)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, AimValue);
        }
    }
}
