using System;
using UnityEngine;

public interface IInputSource
{
    Vector2 MovementValue { get; }
    Vector2 AimValue { get; }

    bool IsFirePressed { get; }
    bool IsDashPressed { get; }

    Event OnInteractionBegin { get; set; }
    Event OnInteractionEnd { get; set; }

    PlayerIdentifier InputIdentifier { get; }
}
