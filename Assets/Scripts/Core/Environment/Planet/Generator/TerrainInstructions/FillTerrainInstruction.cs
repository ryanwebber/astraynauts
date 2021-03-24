using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FillTerrainInstruction : ITerrainInstruction
{
    public class FillPallet
    {
        public TileBase baseTile;
    }

    private FillPallet pallet;

    public FillTerrainInstruction(FillPallet pallet)
    {
        this.pallet = pallet;
    }

    public void Paint(PlanetRegionShape shape, IPlanetRegionTilePainter painter)
    {
        var baseLayer = painter.GetLayer();

        var bounds = shape.VisibleShape.BoundingRect;
        var minTileCoordinate = shape.GetTilemapCoordinate(bounds.min);
        var maxTileCoordinate = shape.GetTilemapCoordinate(bounds.max);

        for (int y = minTileCoordinate.y; y < maxTileCoordinate.y + 1; y++)
        {
            for (int x = minTileCoordinate.x; x < maxTileCoordinate.x + 1; x++)
            {
                var coordinate = new Vector2Int(x, y);
                if (shape.IsTileVisible(coordinate))
                    baseLayer.Tilemap.SetTile((Vector3Int)coordinate, pallet.baseTile);
            }
        }
    }
}
