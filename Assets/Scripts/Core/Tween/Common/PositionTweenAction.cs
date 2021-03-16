using System;
using System.Collections;
using UnityEngine;

public class PositionTweenAction: ITweenAction
{
    private Transform target;
    private Tween<Vector2> tween;

    public PositionTweenAction(Transform target, Tween<Vector2> tween)
    {
        this.target = target;
        this.tween = tween;
    }

    public IEnumerable GetEnumerable()
    {
        return tween.Bind(position => target.position = position);
    }
}
