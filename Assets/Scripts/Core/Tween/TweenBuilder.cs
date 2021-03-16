using System;
using System.Collections;
using UnityEngine;

public class TweenBuilder
{
    private TweenStep tweenStep;

    public TweenBuilder()
    {
    }

    public TweenBuilder Then(ITweenAction tweenAction)
    {
        tweenStep = tweenStep.Then(tweenAction.GetEnumerable());
        return this;
    }

    public TweenBuilder ThenMove(Transform transform, Vector2 position, float duration, AnimationCurve curve)
    {
        var tweenable = new VectorTweenable(transform.position, to: position);
        var tween = new Tween<Vector2>(tweenable, duration, curve);
        var action = new PositionTweenAction(transform, tween);
        return Then(action);
    }

    public TweenBuilder ThenDo(System.Action action)
    {
        return Then(new EventTweenAction(action));
    }

    public Coroutine Start(MonoBehaviour behaviour)
    {
        return behaviour.StartCoroutine(tweenStep.Stacked.GetEnumerator());
    }

    public IEnumerator Build()
    {
        return tweenStep.Stacked.GetEnumerator();
    }
}
