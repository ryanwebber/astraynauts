using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public struct SceneIdentifier
{
    public static SceneIdentifier PERSISTANT = new SceneIdentifier("PersistantScene");
    public static SceneIdentifier PLANET_SURFACE = new SceneIdentifier("PlanetSurfaceScene");

    public readonly string name;

    private SceneIdentifier(string name)
    {
        this.name = name;
    }

    public static implicit operator string(SceneIdentifier identifier)
    {
        return identifier.name;
    }
}
