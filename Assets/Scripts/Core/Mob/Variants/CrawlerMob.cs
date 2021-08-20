using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MobLifecycleController))]
[RequireComponent(typeof(SwarmMovementController))]
public class CrawlerMob : MonoBehaviour
{
    private struct States
    {
        public State swarmState;
        public State leapState;

        public static States FromController(CrawlerMob controller)
        {
            return new States
            {
                swarmState = new ComponentActivationState<SwarmMovementController>(controller.swarmController, "SwarmState"),
                leapState = new EmptyState("LeapState")
            };
        }
    }

    [System.Serializable]
    private struct Interval
    {
        [SerializeField]
        [Min(0)]
        public float min;

        [SerializeField]
        [Min(0)]
        public float max;

        public void Validate() => max = Mathf.Max(max, min);
    }

    [SerializeField]
    private StateIndicator stateIndicator;

    [SerializeField]
    private Interval entitySearchPollDelay;

    [SerializeField]
    private float entitySearchRadius;

    [SerializeField]
    private LayerMask entityLayermask;

    private Event<Collider2D> OnTargetEntityBegin;

    private SwarmMovementController swarmController;
    private StateMachine<States> stateMachine;

    private void Awake()
    {
        swarmController = GetComponent<SwarmMovementController>();
        var lifecycle = GetComponent<MobLifecycleController>();

        stateMachine = new StateMachine<States>(States.FromController(this), states =>
        {
            return lifecycle.Bind(() => stateMachine, states.swarmState);
        });

        stateIndicator?.Bind(stateMachine);

        StartCoroutine(SearchForPlayer());
    }

    private IEnumerator SearchForPlayer()
    {
        var results = new Collider2D[Globals.MAX_PLAYER_COUNT];

        while (true)
        {
            var waitTime = Random.Range(entitySearchPollDelay.min, entitySearchPollDelay.max);
            yield return new WaitForSeconds(waitTime);

            int numCollisions = Physics2D.OverlapCircleNonAlloc(transform.position, entitySearchRadius, results, entityLayermask);
            if (numCollisions > 0)
            {
                var chosenTargetIndex = Random.Range(0, numCollisions);
                var chosenCollider = results[chosenTargetIndex];
                Debug.DrawLine(transform.position, chosenCollider.transform.position, Color.red, 0.5f);
                OnTargetEntityBegin?.Invoke(chosenCollider);
            }
        }
    }

    private void OnValidate()
    {
        entitySearchPollDelay.Validate();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 1f, 1f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, entitySearchRadius);
    }
}
