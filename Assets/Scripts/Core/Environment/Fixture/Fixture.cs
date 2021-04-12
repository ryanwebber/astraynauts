using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fixture : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer[] foregroundSprites;

    [SerializeField]
    private ActionReceiver obstructionArea;

    [Header("Obstruction Animation")]

    [SerializeField]
    private float obstructedOpacity = 0.5f;

    [SerializeField]
    private float fadeDuration = 0.25f;

    private Coroutine currentAnimation;

    private void Awake()
    {
        if (obstructionArea != null && foregroundSprites.Length > 0)
            obstructionArea.OnAnyTriggersChanged += OnObstructionStateChanged;

        foreach (var sprite in foregroundSprites)
            sprite.color = Color.white;
    }

    private void OnObstructionStateChanged(ActionReceiver trigger)
    {
        if (currentAnimation != null)
            StopCoroutine(currentAnimation);

        float currentOpacity = Mathf.Clamp01(foregroundSprites[0].color.a);
        float targetOpacity = Mathf.Clamp01(trigger.IsTriggered ? obstructedOpacity : 1f);
        float durationScale = Mathf.Abs(currentOpacity - targetOpacity);

        if (durationScale > 0)
        {
            float duration = fadeDuration * durationScale;
            currentAnimation = TweenBuilder.WaitSeconds(0f)
                .ThenFade(foregroundSprites, currentOpacity, targetOpacity, duration)
                .Start(this);
        }
    }
}
