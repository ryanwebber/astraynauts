using System;
using UnityEngine;

[CreateAssetMenu(fileName = "WorldShape", menuName = "Custom/Parameters/World Shape")]
public class WorldShapeParameters: ScriptableObject
{
    [SerializeField]
    private WorldGenerator.Parameters parameters;
    public WorldGenerator.Parameters Parameters => parameters;
}
