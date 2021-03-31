using System;
using System.Collections.Generic;
using Extensions;
using UnityEngine;

public class PlanetRegionShape
{
    private Pentagon visibleShape;
    private float cellSize;
    private Lazy<IReadOnlyList<Vector2>> visibleShapePolygon;

    public Pentagon VisibleShape => visibleShape;
    public float CellSize => cellSize;

    public PlanetRegionShape(Pentagon visibleShape, float cellSize)
    {
        this.visibleShape = visibleShape;
        this.cellSize = cellSize;
        this.visibleShapePolygon = new Lazy<IReadOnlyList<Vector2>>(() =>
        {
            return new List<Vector2>(visibleShape.GetPoints());
        });
    }

    public PlanetRegionOrientation Orientation
    {
        get
        {
            float simplifiedRotation = visibleShape.rotation % (2 * Mathf.PI);
            if (simplifiedRotation > 3 * Mathf.PI / 2 || simplifiedRotation < Mathf.PI / 2)
                return PlanetRegionOrientation.NorthFacing;
            else
                return PlanetRegionOrientation.SouthFacing;
        }
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
