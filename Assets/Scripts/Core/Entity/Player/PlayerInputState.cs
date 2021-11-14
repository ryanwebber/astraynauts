using UnityEngine;
using System.Collections;

public class PlayerInputState : MonoBehaviour
{
    [SerializeField, ReadOnly]
    public Vector2 MovementDirection;

    [SerializeField, ReadOnly]
    public Vector2 AimDirection;

    [SerializeField, ReadOnly]
    public bool IsDashing;

    [SerializeField, ReadOnly]
    public bool IsFiring;
}
