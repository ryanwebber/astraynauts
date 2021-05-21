using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SceneReference", menuName = "Custom/Scene/Scene Reference")]
public class SceneReference : ScriptableObject
{
    [SerializeField]
    private string sceneName;
    public SceneIdentifier SceneIdentifier => new SceneIdentifier(sceneName);
}
