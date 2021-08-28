using UnityEngine;
using System.Collections;

[RequireComponent(typeof(ComponentBehavior))]
public class DashActor : MonoBehaviour, BehaviorControlling
{
    [System.Serializable]
    private struct Properties
    {
        public float dashSpeed;
        public float dashDuration;
        public float jumpHeight;
    }

    [SerializeField]
    private Properties properties;

    [SerializeField]
    private Height2D heightComponent;

    [SerializeField]
    private KinematicBody kinematicBody;

    private Coroutine currentDash;
    private ComponentBehavior behavior;

    public Event OnDashStart;
    public Event OnDashEnd;

    public bool IsDashing => currentDash != null;
    public ComponentBehavior Behavior => behavior;

    private void Awake()
    {
        behavior = GetComponent<ComponentBehavior>()
            .BindOnDisable((ref Event ev) =>
            {
                ev += MaybeCancelDash;
            });
    }

    public void DashInDirection(Vector2 direction)
    {
        behavior.MakeCurrent();
        MaybeCancelDash();
        currentDash = StartCoroutine(DoDash(direction.normalized));
    }

    private void MaybeCancelDash()
    {
        if (currentDash != null)
        {
            StopCoroutine(currentDash);
            ResetDashState();
            OnDashEnd?.Invoke();
            currentDash = null;
        }
    }

    private void ResetDashState()
    {
        heightComponent.Height = 0f;
    }

    private IEnumerator DoDash(Vector2 direction)
    {
        OnDashStart?.Invoke();

        var startTime = Time.time;
        while (Time.time < startTime + properties.dashDuration)
        {
            float t = Mathf.Clamp01(Mathf.InverseLerp(startTime, startTime + properties.dashDuration, Time.time));
            heightComponent.Height = PhysicsUtils.LerpGravity(t, properties.jumpHeight);

            kinematicBody.MoveAndCollide(direction * Time.deltaTime * properties.dashSpeed);
            yield return null;
        }

        ResetDashState();
        OnDashEnd?.Invoke();
        currentDash = null;
    }
}
