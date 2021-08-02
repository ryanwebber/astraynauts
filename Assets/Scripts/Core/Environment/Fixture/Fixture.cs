using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Polygon))]
public class Fixture : MonoBehaviour
{
    private Polygon bounds;

    private void Awake()
    {
        bounds = GetComponent<Polygon>();
    }

    public void RegisterInWorld(WorldGrid grid)
    {
        foreach (var point in bounds.Points)
        {
            var worldPosition = (Vector2)transform.position + point;
            var unit = new Vector2Int(Mathf.FloorToInt(worldPosition.x), Mathf.FloorToInt(worldPosition.y));
            var descriptor = new FixtureDescriptor(this);
            grid.AddDescriptor(unit, descriptor);
        }
    }
}
