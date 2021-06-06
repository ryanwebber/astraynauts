using System;
using UnityEngine;
using static GameManager;

public class GameState: MonoBehaviour
{
    public Event OnGameStateInitializationBegin;
    public Event OnGameStateInitializationEnd;

    [SerializeField]
    private Services services;
    public Services Services => services;

    private World world;
    public World World => world;

    private Parameters parameters;
    public Parameters SceneParameters => parameters;

    public void InitializeInBlock(World world, Parameters parameters, System.Action block)
    {
        this.world = world;
        this.parameters = parameters;

        OnGameStateInitializationBegin?.Invoke();
        block?.Invoke();
        OnGameStateInitializationEnd?.Invoke();
    }
}
