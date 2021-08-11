using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Projectile))]
[RequireComponent(typeof(Velocity2D))]
[RequireComponent(typeof(DamageDealer))]
[RequireComponent(typeof(DestructionTrigger))]
public class BulletLikeProjectile : MonoBehaviour
{
    [SerializeField]
    private float constantVeclocity;

    [SerializeField]
    private LayerMask collisionMask;

    private bool IsMoving = true;

    private DamageDealer damageDealer;
    private DestructionTrigger destructionTrigger;
    private Velocity2D headingSource;

    private Vector2 CurrentVelocity
    {
        get => headingSource.CurrentVelocity;
        set => headingSource.CurrentVelocity = value.normalized * constantVeclocity;
    }

    private void Awake()
    {
        damageDealer = GetComponent<DamageDealer>();
        destructionTrigger = GetComponent<DestructionTrigger>();
        headingSource = GetComponent<Velocity2D>();

        destructionTrigger.OnDestructionTriggered += () => IsMoving = false;
        GetComponent<Projectile>().OnProjectileSpawn += v => CurrentVelocity = v;
    }

    private void Update()
    {
        if (IsMoving)
        {
            var oldPosition = transform.position;
            transform.Translate(Time.deltaTime * CurrentVelocity);
            var newPosition = transform.position;

            var collision = Physics2D.Linecast(oldPosition, newPosition, collisionMask);
            if (collision)
            {
                HandleImpact(collision.collider);
            }
        }
    }

    public void OnTriggerEnter2D(Collider2D collider)
    {
        if (((1 << collider.gameObject.layer) & collisionMask) != 0)
        {
            HandleImpact(collider);
        }
    }

    private void HandleImpact(Collider2D collider)
    {
        Debug.Log($"Bullet trigger: {collider.name}");
        
        if (damageDealer.TryDealDamage(collider.gameObject, out var result))
        {
            if (result.totalDamageDealt > 0 || result.targetWasShielded)
                destructionTrigger.DestroyWithBehaviour();
        }
        else
        {
            destructionTrigger.DestroyWithBehaviour();
        }
    }
}
