using System;
using UnityEngine;

[System.Serializable]
public struct  WalkingProperties
{
    public float movementSpeed;
    public float movementDampening;

    public float EvaluateSpeed(Vector2 direction)
    {
        return movementSpeed;
    }
}
