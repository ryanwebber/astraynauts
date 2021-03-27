using UnityEngine;
using System.Collections;

public class CollisionReceiver : MonoBehaviour
{
    public Event<CollisionTrigger> OnCollisionTrigger;

    [SerializeField]
    private LayerMask triggerMask;
    public LayerMask TriggerMask => triggerMask;
}
