using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Grid))]
public class SerializedTilemap : MonoBehaviour
{
    [SerializeField]
    private Tilemap tilemapLayer;

    public TilemapTemplate Template
    {
        get
        {
            tilemapLayer.CompressBounds();
            return TilemapTemplate.From(tilemapLayer);
        }
    }

    private void Start()
    {
        Assert.IsTrue(false, "TilemapTemplate should not be initialized into a scene");
        Destroy(this);
    }
}
