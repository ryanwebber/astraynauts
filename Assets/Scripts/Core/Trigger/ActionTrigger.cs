using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class ActionTrigger : MonoBehaviour
{
    public Event<ActionTrigger> OnTriggerDestroyed;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.gameObject.TryGetComponent(out ActionReceiver receiver))
        {
            if (IsTriggerInMask(receiver, collision.collider))
            {
                receiver.OnActionTriggerBegin(this);
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.gameObject.TryGetComponent(out ActionReceiver receiver))
        {
            if (IsTriggerInMask(receiver, collision.collider))
            {
                receiver.OnActionTriggerEnd(this);
            }
        }
    }

    private bool IsTriggerInMask(ActionReceiver receiver, Collider2D collider)
    {
        return ((1 << collider.gameObject.layer) & receiver.TriggerMask) != 0;
    }
}
