using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PlayerInputFeedback))]
[RequireComponent(typeof(PlayerInputBinder))]
public class Player : MonoBehaviour
{
    public PlayerIdentifier Identifier { get; private set; }
    public PlayerInputBinder InputBinder { get; private set; }
    public PlayerInputFeedback InputFeedback { get; private set; }

    private void Awake()
    {
        Identifier = PlayerIdentifier.Default;
        InputFeedback = GetComponent<PlayerInputFeedback>();
        InputBinder = GetComponent<PlayerInputBinder>();
        InputBinder.OnAttachToInput += input => Identifier = input.InputIdentifier;
    }
}
