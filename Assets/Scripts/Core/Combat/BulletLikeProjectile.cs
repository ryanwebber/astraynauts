using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Velocity2D))]
[RequireComponent(typeof(Projectile))]
public class BulletLikeProjectile : MonoBehaviour
{
    [SerializeField]
    private float constantVeclocity;

    [SerializeField]
    private LayerMask collisionMask;

    private Velocity2D headingSource;
    private Vector2 CurrentVelocity
    {
        get => headingSource.CurrentVelocity;
        set => headingSource.CurrentVelocity = value.normalized * constantVeclocity;
    }

    private void Awake()
    {
        headingSource = GetComponent<Velocity2D>();
        GetComponent<Projectile>().OnProjectileSpawn += v =>
        {
            CurrentVelocity = v;
        };
    }

    private void Update()
    {
        transform.Translate(Time.deltaTime * CurrentVelocity);
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & collisionMask) != 0)
        {
            Debug.Log("Projectile collision");
        }
    }
}
