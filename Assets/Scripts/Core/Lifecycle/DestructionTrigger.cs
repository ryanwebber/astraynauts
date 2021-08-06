using UnityEngine;
using System.Collections;

public class DestructionTrigger : MonoBehaviour
{
    public Event OnDestructionTriggered;

    public void DestroyWithBehaviour()
        => OnDestructionTriggered?.Invoke();
}
