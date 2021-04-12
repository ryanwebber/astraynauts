using System;
using System.Collections;
using UnityEngine;

public class ScaleTweenAction : ITweenAction
{
    private Transform target;
    private Tween<Vector2> tween;

    public ScaleTweenAction(Transform target, Tween<Vector2> tween)
    {
        this.target = target;
        this.tween = tween;
    }

    public IEnumerator GetYieldInstructions()
    {
        return tween.Bind(scale => target.localScale = scale);
    }
}
