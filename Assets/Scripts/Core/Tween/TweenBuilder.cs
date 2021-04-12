using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TweenBuilder
{
    private List<IEnumerator> enumerators;

    private TweenBuilder()
    {
        enumerators = new List<IEnumerator>();
    }

    public TweenBuilder Then(ITweenAction tweenAction)
    {
        enumerators.Add(tweenAction.GetYieldInstructions());
        return this;
    }

    public TweenBuilder ThenWait(float duration)
    {
        enumerators.Add(Coroutines.After(duration, () => { }));
        return this;
    }

    public TweenBuilder ThenMove(Transform transform, Vector2 position, float duration, ITweenCurve curve = default)
    {
        var tweenable = new VectorTweenable(from: transform.position, to: position);
        var tween = new Tween<Vector2>(tweenable, duration, curve);
        var action = new PositionTweenAction(transform, tween);
        return Then(action);
    }

    public TweenBuilder ThenScale(Transform transform, Vector2 scale, float duration, ITweenCurve curve = default)
    {
        var tweenable = new VectorTweenable(from: transform.localScale, to: scale);
        var tween = new Tween<Vector2>(tweenable, duration, curve);
        var action = new ScaleTweenAction(transform, tween);
        return Then(action);
    }

    public TweenBuilder ThenFade(SpriteRenderer renderer, float opacity, float duration, ITweenCurve curve = default)
    {
        var tweenable = new FloatTweenable(from: renderer.color.a, to: opacity);
        var tween = new Tween<float>(tweenable, duration, curve);
        var action = new OpacityTweenAction(renderer, tween);
        return Then(action);
    }

    public TweenBuilder ThenFade(IEnumerable<SpriteRenderer> renderers, float from, float to, float duration, ITweenCurve curve = default)
    {
        var tweenable = new FloatTweenable(from: from, to: to);
        var tween = new Tween<float>(tweenable, duration, curve);
        var action = new OpacityTweenAction(renderers, tween);
        return Then(action);
    }

    public TweenBuilder ThenDo(Action action)
    {
        return Then(new EventTweenAction(action));
    }

    public Coroutine Start(MonoBehaviour behaviour)
    {
        return behaviour.StartCoroutine(Build());
    }

    public IEnumerator Build()
    {
        foreach (var enumerator in enumerators)
        {
            while (enumerator.MoveNext())
                yield return enumerator.Current;
        }
    }

    public static TweenBuilder WaitSeconds(float duration)
    {
        return new TweenBuilder()
            .ThenWait(duration);
    }
}
