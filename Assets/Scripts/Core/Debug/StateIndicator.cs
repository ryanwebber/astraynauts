using UnityEngine;
using System.Collections;

public class StateIndicator : MonoBehaviour
{
    [SerializeField]
    private Vector2 offset;

    [SerializeField]
    private float size = 0.1f;

    [SerializeField]
    [ReadOnly]
    private string state;

    public string State
    {
        get => state;
        set => SetState(value);
    }

    public void SetState(string value)
    {
        state = value;
    }

    public void Bind<T>(StateMachine<T> stateMachine)
    {
        stateMachine.OnStateChanged += (_, newState) =>
        {
            SetState(newState.Name);
        };

        SetState(stateMachine.CurrentState.Name);
    }

    private void OnDrawGizmos()
    {
        Color color;
        if (state == null || state.Length == 0)
        {
            color = Color.white;
        }
        else
        {
            var hue = Mathf.InverseLerp(0, 255, state.GetHashCode() % 255);
            color = Color.HSVToRGB(hue, 1f, 1f);
        }

        Gizmos.color = color;
        Gizmos.DrawSphere((Vector2)transform.position + offset, size);
    }
}
