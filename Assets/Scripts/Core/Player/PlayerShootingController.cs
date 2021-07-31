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
    private BatteryManager batteryManager;

    [SerializeField]
    private Properties properties;

    public Vector2 AimInputValue;
    public Event OnFireInputBegin;
    public Event OnFireInputEnd;

    private ProjectileSpawner projectileSpawner;
    private PlayerInputFeedback inputFeedback;

    public bool IsShootingLocked { get; set; } = false;

    private void Awake()
    {
        inputFeedback = GetComponent<PlayerInputFeedback>();
        projectileSpawner = GetComponent<ProjectileSpawner>();

        OnFireInputBegin += () => LoadProjectile();
        OnFireInputEnd += ()  => FireProjectile();

        projectileSpawner.Decorators += projectile =>
        {
            // TODO: Player-based decoration
        };
    }

    private void LoadProjectile()
    {
        if (IsShootingLocked)
            return;
    }

    private void FireProjectile()
    {
        if (IsShootingLocked)
            return;

        if (batteryManager.BatteryValue <= 0)
            return;

        inputFeedback.TriggerHapticInstant();
        projectileSpawner.SpawnProjectile(AimInputValue);

        batteryManager.AddBatteryValue(-1);
    }

    private void OnDrawGizmos()
    {
        if (AimInputValue.sqrMagnitude > 0.001f)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, AimInputValue);
        }
    }
}
