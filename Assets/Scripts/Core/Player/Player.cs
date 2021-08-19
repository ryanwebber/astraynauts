﻿using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PlayerState))]
[RequireComponent(typeof(PlayerInputFeedback))]
[RequireComponent(typeof(PlayerInputBinder))]
public class Player : MonoBehaviour
{
    public Event<GameState> OnPlayerWillSpawn;

    public PlayerIdentifier Identifier { get; private set; }
    public PlayerInputBinder InputBinder { get; private set; }
    public PlayerInputFeedback InputFeedback { get; private set; }

    public PlayerState State { get; private set; }
    public Vector2 WorldPosition => transform.position;

    private void Awake()
    {
        Identifier = PlayerIdentifier.Default;

        InputFeedback = GetComponent<PlayerInputFeedback>();
        InputBinder = GetComponent<PlayerInputBinder>();
        State = GetComponent<PlayerState>();

        InputBinder.OnAttachToInput += input => Identifier = input.InputIdentifier;
    }
}
