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
    }

    [SerializeField]
    private Properties properties;

    public Event OnDashStart;
    public Event OnDashEnd;

    private KinematicBody kinematicBody;
    private Coroutine currentDash;

    private void Awake()
    {
        kinematicBody = GetComponent<KinematicBody>();
        OnDashEnd += () => currentDash = null;
    }

    public void DashInDirection(Vector2 direction)
    {
        if (currentDash != null)
        {
            StopCoroutine(currentDash);
        }

        currentDash = StartCoroutine(DoDash(direction.normalized));
    }

    private IEnumerator DoDash(Vector2 direction)
    {
        OnDashStart?.Invoke();

        var startTime = Time.time;
        while (Time.time < startTime + properties.dashDuration)
        {
            kinematicBody.MoveAndCollide(direction * Time.deltaTime * properties.dashSpeed);
            yield return null;
        }

        OnDashEnd?.Invoke();
    }
}
