using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PlayerInputBinder))]
public class Player : MonoBehaviour
{
    public PlayerInputBinder InputBinder { get; private set; }

    private void Awake()
    {
        InputBinder = GetComponent<PlayerInputBinder>();
    }
}
