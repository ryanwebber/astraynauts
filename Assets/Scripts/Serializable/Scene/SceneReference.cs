using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SceneReference", menuName = "Custom/Scene/Scene Reference")]
public class SceneReference : ScriptableObject
{
    private SceneReference scene;
    public SceneReference Scene;

    public static implicit operator SceneIdentifier(SceneReference sceneReference)
    {
        return sceneReference.Scene;
    }
}
