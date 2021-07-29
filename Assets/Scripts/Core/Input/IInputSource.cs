using System;
using UnityEngine;

public interface IInputSource
{
    Event OnFireBegin { get; set; }
    Event OnFireEnd { get; set; }

    Event OnInteractionBegin { get; set; }
    Event OnInteractionEnd { get; set; }

    Event OnMovementSpecialAction { get; set; }

    Vector2 MovementValue { get; }
    Vector2 AimValue { get; }

    PlayerIdentifier InputIdentifier { get; }
}
