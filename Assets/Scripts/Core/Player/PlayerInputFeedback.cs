using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(PlayerInputBinder))]
public class PlayerInputFeedback : MonoBehaviour
{
    private IInputFeedback feedback;

    private void Awake()
    {
        GetComponent<PlayerInputBinder>().OnAttatchToFeedback += feedback =>
        {
            this.feedback = feedback;
        };

        this.feedback = InputFeedbackUtils.DetachedFeedback;
    }

    public void TriggerHapticInstant()
    {
        feedback.OnTriggerHapticFeedback?.Invoke();
    }

    public void TriggerHapticSession(IEnumerable<float> stream)
    {
        StartCoroutine(StreamHaptics(stream));
    }

    private IEnumerator StreamHaptics(IEnumerable<float> stream)
    {
        foreach (var value in stream)
        {
            // TODO: set haptic value
            yield return null;
        }
    }
}
