using System;
using System.Collections.Generic;
using Extensions;
using UnityEngine;

public class PlanetRegionShape
{
    private Shape visibleShape;
    private float cellSize;
    private Lazy<IReadOnlyList<Vector2>> visibleShapePolygon;

    public readonly PlanetRegionOrientation Orientation;

    public Shape VisibleShape => visibleShape;
    public float CellSize => cellSize;

    public PlanetRegionShape(Shape visibleShape, float cellSize, PlanetRegionOrientation orientation)
    {
        this.visibleShape = visibleShape;
        this.cellSize = cellSize;
        this.Orientation = orientation;
        this.visibleShapePolygon = new Lazy<IReadOnlyList<Vector2>>(() =>
        {
            return new List<Vector2>(visibleShape.GetPoints());
        });
    }

    public bool IsTileVisible(Vector2Int coordinates)
    {
        var tileBounds = GetTileBounds(coordinates);
        Vector2[] tilePolygon = new[] {
            tileBounds.GetTopLeft(),
            tileBounds.GetTopRight(),
            tileBounds.GetBottomRight(),
            tileBounds.GetBottomLeft(),
        };

        return PolygonUtils.ConvexOverlap(tilePolygon, visibleShapePolygon.Value);
    }

    public Bounds GetTileBounds(Vector2Int coordinates)
    {
        var center = new Vector2(coordinates.x, coordinates.y) * cellSize + (Vector2.one * cellSize / 2f);
        return new Bounds(center, Vector2.one * cellSize);
    }

    public Vector2Int GetTilemapCoordinate(Vector2 position)
    {
        int x = Mathf.FloorToInt(position.x / cellSize);
        int y = Mathf.FloorToInt(position.y / cellSize);
        return new Vector2Int(x, y);
    }
}
