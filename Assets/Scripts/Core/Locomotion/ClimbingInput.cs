using UnityEngine;
using System.Collections;

public class ClimbingInput : MonoBehaviour
{
    [ReadOnly]
    public Vector2 MovementDirection;

    [ReadOnly]
    public bool IsJumping;

    private void LateUpdate()
    {
        IsJumping = false;
    }
}
