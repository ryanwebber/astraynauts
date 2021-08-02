using System;
using System.Collections.Generic;

public class WorldState
{
    public Teleporter PlayerSpawnTeleporter;
    public readonly List<Teleporter> AccessibleTeleporters;

    public WorldState()
    {
        PlayerSpawnTeleporter = null;
        AccessibleTeleporters = new List<Teleporter>();
    }
}
