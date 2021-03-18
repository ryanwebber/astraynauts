using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public struct SceneIdentifier
{
    public static SceneIdentifier PERSISTANT = new SceneIdentifier("Persistant");

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
