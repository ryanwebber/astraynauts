﻿using System;
using UnityEngine;

[System.Serializable]
public struct LocomotionProperties
{
    public float movementSpeed;

    public float EvaluateSpeed(Vector2 direction)
    {
        return movementSpeed;
    }
}
