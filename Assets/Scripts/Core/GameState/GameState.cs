using System;

public class GameState
{
    public readonly World World;
    public readonly GameManager.Services Services;

    public GameState(World world, GameManager.Services services)
    {
        this.World = world;
        this.Services = services;
    }
}
