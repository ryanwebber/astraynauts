using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// TODO: Randomly scattering terrain templates doesn't look good.
// What we should do is 'ring' these around objects like rocks,
// so they lead into the texture of the ground
public class AddNoiseTerrainInstruction : ITerrainInstruction
{
    public struct Pallet
    {
        public RandomAccessCollection<TilemapTemplate> templates;
    }

    private Pallet pallet;
    private float density;

    public AddNoiseTerrainInstruction(Pallet pallet, float density)
    {
        this.pallet = pallet;
        this.density = density;
    }

    public void Paint(PlanetRegionShape shape, IPlanetRegionTilePainter painter)
    {
        if (pallet.templates.Count == 0)
            return;

        var baseLayer = painter.GetLayer();

        var bounds = shape.VisibleShape.BoundingRect;
        var minTileCoordinate = shape.GetTilemapCoordinate(bounds.min);
        var maxTileCoordinate = shape.GetTilemapCoordinate(bounds.max);

        HashSet<Vector3Int> usedPositions = new HashSet<Vector3Int>();

        for (int i = 0; i < (maxTileCoordinate - minTileCoordinate).magnitude * density; i++)
        {
            int x = UnityEngine.Random.Range(minTileCoordinate.x, maxTileCoordinate.x);
            int y = UnityEngine.Random.Range(minTileCoordinate.y, maxTileCoordinate.y);
            var position = new Vector3Int(x, y, 0);
            var template = pallet.templates.NextValue();

            bool isValid = true;
            foreach (var offset in template)
            {
                if (usedPositions.Contains(position + offset))
                {
                    isValid = false;
                    break;
                }

                if (!shape.IsTileVisible(position + offset))
                {
                    isValid = false;
                    break;
                }
            }

            if (!isValid)
                continue;

            foreach (var offset in template)
            {
                var cellPosition = position + offset;
                baseLayer.Tilemap.SetTile(cellPosition, template.GetTile(offset));
                usedPositions.Add(cellPosition);
            }
        }
    }
}
