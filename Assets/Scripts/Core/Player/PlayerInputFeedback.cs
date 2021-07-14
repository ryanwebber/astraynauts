using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PlayerInputBinder))]
public class PlayerInputFeedback : MonoBehaviour
{
    private IInputFeedback feedback;
    public IInputFeedback CurrentFeedback => feedback;

    private void Awake()
    {
        GetComponent<PlayerInputBinder>().OnAttatchToFeedback += feedback =>
        {
            this.feedback = feedback;
        };

        this.feedback = InputFeedbackUtils.DetachedFeedback;
    }
}
