using UnityEngine;
using System.Collections;

public class ActionReceiver : MonoBehaviour
{
    public Event<ActionTrigger> OnActionTriggerBegin;
    public Event<ActionTrigger> OnActionTriggerEnd;
    public Event<ActionReceiver> OnAnyTriggersChanged;

    [SerializeField]
    private LayerMask triggerMask;
    public LayerMask TriggerMask => triggerMask;

    private bool previouslyTriggered = false;
    public bool IsTriggered => previouslyTriggered;

    private Collider2D triggerCollider;

    private void Awake()
    {
        triggerCollider = GetComponent<Collider2D>();
    }

    private void Update()
    {
        var isTriggered = triggerCollider.IsTouchingLayers(triggerMask);
        if (isTriggered != previouslyTriggered)
        {
            previouslyTriggered = isTriggered;
            OnAnyTriggersChanged?.Invoke(this);
        }
    }
}
