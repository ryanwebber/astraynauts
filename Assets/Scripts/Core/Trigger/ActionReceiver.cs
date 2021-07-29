using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ActionReceiver : MonoBehaviour
{
    public class NonAllocRequest
    {
        public readonly Collider2D[] collisions;
        public readonly ActionTrigger[] triggers;

        public NonAllocRequest(int maxResults)
        {
            collisions = new Collider2D[maxResults];
            triggers = new ActionTrigger[maxResults];
        }
    }    

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

    public int GetTriggers(NonAllocRequest request)
    {
        var filter = new ContactFilter2D();
        filter.SetLayerMask(triggerMask);

        int numCollisions = triggerCollider.OverlapCollider(filter, request.collisions);
        int numTriggers = 0;
        for (int i = 0; i < numCollisions; i++)
        {
            if (request.collisions[i].TryGetComponent<ActionTrigger>(out var trigger))
            {
                request.triggers[numTriggers++] = trigger;
            }
        }

        return numTriggers;
    }
}
