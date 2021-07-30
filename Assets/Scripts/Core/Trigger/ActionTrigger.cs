using UnityEngine;
using System.Collections;

public class ActionTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (TestCollision(collider, out var receiver))
        {
            receiver.OnActionTriggerBegin(this);
        }
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        if (TestCollision(collider, out var receiver))
        {
            receiver.OnActionTriggerEnd(this);
        }
    }

    private bool TestCollision(Collider2D collider, out ActionReceiver receiver)
    {
        if (collider.gameObject.TryGetComponent(out receiver))
        {
            if (((1 << this.gameObject.layer) & receiver.TriggerMask) != 0)
            {
                return true;
            }
        }

        return false;
    }
}
