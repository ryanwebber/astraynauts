using UnityEngine;
using System.Collections;

[RequireComponent(typeof(KinematicBody))]
public class CollisionTrigger : MonoBehaviour
{
    private void Awake()
    {
        var body = GetComponent<KinematicBody>();
        body.OnCollision += OnKinematicCollision;
    }

    private void OnKinematicCollision(Collider2D collider, KinematicBody.Collision collision)
    {
        foreach (var collisionInstance in collision.GetCollisions())
        {
            if (collisionInstance.collider.gameObject.TryGetComponent(out CollisionReceiver receiver))
            {
                if (IsTriggerInMask(receiver, collider))
                {
                    receiver.OnCollisionTrigger(this);
                }
            }
        }
    }

    private bool IsTriggerInMask(CollisionReceiver receiver, Collider2D collider)
    {
        return ((1 << collider.gameObject.layer) & receiver.TriggerMask) != 0;
    }
}
