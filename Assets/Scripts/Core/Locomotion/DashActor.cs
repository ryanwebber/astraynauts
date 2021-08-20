using UnityEngine;
using System.Collections;

[RequireComponent(typeof(KinematicBody))]
public class DashActor : MonoBehaviour
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
    private Transform entitySprite;

    public Event OnDashStart;
    public Event OnDashEnd;

    private KinematicBody kinematicBody;
    private Coroutine currentDash;

    private void Awake()
    {
        kinematicBody = GetComponent<KinematicBody>();
        OnDashEnd += () =>
        {
            currentDash = null;
            ResetDashState();
        };
    }

    public void DashInDirection(Vector2 direction)
    {
        if (currentDash != null)
        {
            StopCoroutine(currentDash);
            ResetDashState();
        }

        currentDash = StartCoroutine(DoDash(direction.normalized));
    }

    private void ResetDashState()
    {
        entitySprite.transform.localPosition = Vector3.zero;
    }

    private IEnumerator DoDash(Vector2 direction)
    {
        OnDashStart?.Invoke();

        var startTime = Time.time;
        while (Time.time < startTime + properties.dashDuration)
        {
            float t = Mathf.Clamp01(Mathf.InverseLerp(startTime, startTime + properties.dashDuration, Time.time));
            entitySprite.transform.localPosition = PhysicsUtils.LerpGravity(t, properties.jumpHeight);

            kinematicBody.MoveAndCollide(direction * Time.deltaTime * properties.dashSpeed);
            yield return null;
        }

        OnDashEnd?.Invoke();
    }
}
