using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpacityTweenAction: ITweenAction
{
    private IEnumerable<SpriteRenderer> renderers;
    private Tween<float> tween;

    public OpacityTweenAction(SpriteRenderer renderer, Tween<float> tween)
    {
        this.renderers = new SpriteRenderer[] { renderer };
        this.tween = tween;
    }

    public OpacityTweenAction(IEnumerable<SpriteRenderer> renderers, Tween<float> tween)
    {
        this.renderers = renderers;
        this.tween = tween;
    }

    public IEnumerator GetYieldInstructions()
    {
        return tween
            .Map(opacity => new Color(1f, 1f, 1f, opacity))
            .Bind(color =>
            {
                foreach (var renderer in this.renderers)
                    renderer.color = color;
            });
    }
}
