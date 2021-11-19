using UnityEngine;
using System.Collections;

public class GridLockedBody : MonoBehaviour
{
    public Event<Vector2, Vector2> OnMoveBegin;
    public Event<Vector2, Vector2> OnMoveEnd;

    [SerializeField]
    private float movementTime = 0.2f;

    [SerializeField]
    private Vector2 offset;

    public bool IsInitialized => world != null && traversable != null;
    public bool IsMoving => currentMovement != null;
    public Vector2 WorldPosition
    {
        get => (Vector2)transform.position + offset;
        private set
        {
            var targetPos = value - offset;
            transform.position = targetPos;
        }
    }

    private World world;
    private ITraversableGrid traversable;
    private Coroutine currentMovement;

    public void InitializeInWorld(World world)
    {
        this.world = world;
        this.traversable = new WorldGridTraversable(world.Grid);
    }

    public void MoveBy(Vector2Int offset)
    {
        if (!IsInitialized)
            return;

        var currentPosition = world.WorldPositionToUnit(WorldPosition);
        var targetPosition = currentPosition + offset;
        MoveTo(targetPosition);
    }

    public void MoveTo(Vector2Int position)
    {
        if (!IsInitialized)
            return;

        if (currentMovement != null)
            return;

        currentMovement = StartCoroutine(MoveToAnimated(world.UnitBounds(position).center));
    }

    private void OnDisable()
    {
        currentMovement = null;
    }

    private IEnumerator MoveToAnimated(Vector2 position)
    {
        float startTime = Time.time;
        float endTime = Time.time + movementTime;

        Vector2 startPos = WorldPosition;
        Vector2 endPos = position;

        OnMoveBegin?.Invoke(startPos, endPos);

        while (Time.time < endTime)
        {
            var t = Mathf.InverseLerp(startTime, endTime, Time.time);
            var p = Vector2.Lerp(startPos, endPos, t);
            WorldPosition = p;
            yield return null;
        }

        WorldPosition = endPos;
        OnMoveEnd?.Invoke(startPos, endPos);

        currentMovement = null;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.gray;
        Gizmos.DrawSphere(WorldPosition, 0.1f);
    }
}
