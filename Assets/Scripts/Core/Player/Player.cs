using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PlayerInputFeedback))]
[RequireComponent(typeof(PlayerInputBinder))]
public class Player : MonoBehaviour
{
    public Event<GameState> OnPlayerWillSpawn;

    [SerializeField]
    private PlayerState playerState;
    public PlayerState State => playerState;

    public PlayerIdentifier Identifier { get; private set; }
    public PlayerInputBinder InputBinder { get; private set; }
    public PlayerInputFeedback InputFeedback { get; private set; }

    public Vector2 WorldPosition => transform.position;

    private void Awake()
    {
        Identifier = PlayerIdentifier.Default;

        InputFeedback = GetComponent<PlayerInputFeedback>();
        InputBinder = GetComponent<PlayerInputBinder>();

        InputBinder.OnAttachToInput += input => Identifier = input.InputIdentifier;
    }
}
