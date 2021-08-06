using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Velocity2D))]
[RequireComponent(typeof(Projectile))]
[RequireComponent(typeof(DestructionTrigger))]
public class BulletLikeProjectile : MonoBehaviour
{
    [SerializeField]
    private float constantVeclocity;

    [SerializeField]
    private LayerMask collisionMask;

    private bool IsMoving = true;

    private DestructionTrigger destructionTrigger;
    private Velocity2D headingSource;
    private Vector2 CurrentVelocity
    {
        get => headingSource.CurrentVelocity;
        set => headingSource.CurrentVelocity = value.normalized * constantVeclocity;
    }

    private void Awake()
    {
        destructionTrigger = GetComponent<DestructionTrigger>();
        headingSource = GetComponent<Velocity2D>();

        destructionTrigger.OnDestructionTriggered += () => IsMoving = false;
        GetComponent<Projectile>().OnProjectileSpawn += v => CurrentVelocity = v;
    }

    private void Update()
    {
        if (IsMoving)
            transform.Translate(Time.deltaTime * CurrentVelocity);
    }

    public void OnTriggerEnter2D(Collider2D collider)
    {
        if (((1 << collider.gameObject.layer) & collisionMask) != 0)
            destructionTrigger.DestroyWithBehaviour();
    }
}
