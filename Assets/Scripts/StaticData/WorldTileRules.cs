using System;
using System.Collections.Generic;
using UnityEngine;
using static WorldLoader;

public static class WorldTileRules
{
    public static ICollection<TileRulePair> GetRules(
        FloorSettings floorSettings,
        PerimeterSettings perimeterSettings,
        WallSettings wallSettings)
    {
        return new TileRulePair[]
        {
            // Room tiles
            new TileRulePair
            {
                condition = new RelativeTileTypeCondition<FloorDescriptor>
                {
                    predicate = descriptor => descriptor.FloorLocation == FloorDescriptor.Location.ROOM
                },
                generator = new TileGenerator
                {
                    source = new RandomTile(floorSettings.roomTiles.AsCollection()),
                    layer = floorSettings.tilemap
                }
            },

            // Hallway tiles
            new TileRulePair
            {
                condition = new RelativeTileTypeCondition<FloorDescriptor>
                {
                    predicate = descriptor => descriptor.FloorLocation == FloorDescriptor.Location.HALLWAY
                },
                generator = new TileGenerator
                {
                    source = new RandomTile(floorSettings.hallTiles.AsCollection()),
                    layer = floorSettings.tilemap
                }
            },

            // Perimiter tiles
            new TileRulePair
            {
                condition = new CompositeTileCondition(
                    new NegatedTileCondition(
                        new RelativeTileTypeCondition<FloorDescriptor>()
                    ),
                    new CompositeTileCondition(Operator.OR,
                        new RelativeTileTypeCondition<FloorDescriptor> { offset = Vector2Int.up },
                        new RelativeTileTypeCondition<FloorDescriptor> { offset = Vector2Int.right },
                        new RelativeTileTypeCondition<FloorDescriptor> { offset = Vector2Int.down },
                        new RelativeTileTypeCondition<FloorDescriptor> { offset = Vector2Int.left },
                        new RelativeTileTypeCondition<FloorDescriptor> { offset = new Vector2Int(1, 1) },
                        new RelativeTileTypeCondition<FloorDescriptor> { offset = new Vector2Int(1, -1) },
                        new RelativeTileTypeCondition<FloorDescriptor> { offset = new Vector2Int(-1, 1) },
                        new RelativeTileTypeCondition<FloorDescriptor> { offset = new Vector2Int(-1, -1) }
                    )
                ),
                generator = new TileGenerator
                {
                    source = new SingleTile(perimeterSettings.collisionTile),
                    layer = perimeterSettings.tilemap
                }
            },

            // Wall tiles
            new TileRulePair
            {
                condition = new CompositeTileCondition(
                    new RelativeTileTypeCondition<FloorDescriptor>(),
                    new RelativeTileTypeCondition<FloorDescriptor> { offset = Vector2Int.left },
                    new RelativeTileTypeCondition<FloorDescriptor> { offset = Vector2Int.right },
                    new NegatedTileCondition(
                        new RelativeTileTypeCondition<FloorDescriptor> { offset = Vector2Int.down }
                    )
                ),
                generator = new TileGenerator
                {
                    source = new SingleTile(wallSettings.southWall),
                    layer = wallSettings.tilemap
                }
            }
        };
    }
}
