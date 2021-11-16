using UnityEngine;
using System.Collections;

public class Stage : MonoBehaviour
{
    public Event OnStageBegin;
    public Event OnStageEnd;

    public bool IsStageActive { get; private set; }

    private void Awake()
    {
        OnStageBegin += () => IsStageActive = true;
        OnStageEnd += () => IsStageActive = false;

        // Debugging
        OnStageBegin += () => Debug.Log($"{name} stage begin", this);
        OnStageEnd += () => Debug.Log($"{name} stage end", this);
    }
}
