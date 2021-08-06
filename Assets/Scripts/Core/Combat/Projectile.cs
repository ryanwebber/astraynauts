using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour
{
    public Event<Vector2> OnProjectileSpawn;

    // TODO: Delete this
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(transform.position, 0.1f);
    }
}
