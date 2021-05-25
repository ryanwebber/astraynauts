using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(WorldLoader))]
public class World : MonoBehaviour
{
    [SerializeField]
    WorldShapeParameters worldShape;

    private WorldLoader loader;

    private void Awake()
    {
        loader = GetComponent<WorldLoader>();
    }

    private void Start()
    {
        // Need to make world generator thread safe (random)
        var layout = WorldGenerator.Generate(worldShape.Parameters);
        loader.LoadWorld(layout, OnWorldLoadComplete);
    }

    private void OnWorldLoadComplete()
    {

    }
}
