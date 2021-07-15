using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Projectile))]
[RequireComponent(typeof(RaycastBody))]
public class LinearTrajectory : MonoBehaviour
{
    [SerializeField]
    public float Veclocity;

    [SerializeField]
    [ReadOnly]
    private Vector2 normalizedHeading;

    public Vector2 Heading
    {
        get => normalizedHeading;
        set => normalizedHeading = value.normalized;
    }

    private RaycastBody body;
    public RaycastBody RaycastBody => body;

    private void Awake()
    {
        body = GetComponent<RaycastBody>();
        GetComponent<Projectile>().OnProjectileSpawn += direction =>
        {
            this.Heading = direction;
        };

        // TODO: remove this
        StartCoroutine(Coroutines.After(10f, () => Destroy(gameObject)));
    }

    private void Update()
    {
        var delta = normalizedHeading * Veclocity * Time.deltaTime;
        var finalHeading = body.MoveAndBounce(delta);

        // Update the heading after the bounce
        Heading = finalHeading;
    }

    private void OnDrawGizmos()
    {
        // TODO: Remove this
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(transform.position, 0.1f);
    }
}
