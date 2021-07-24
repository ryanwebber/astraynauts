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
                generators = new TileGenerator[]
                {
                    new TileGenerator {
                        source = new RandomTile(floorSettings.roomTiles.AsCollection()),
                        layer = floorSettings.tilemap
                    }
                }
            },

            // Hallway tiles

            new TileRulePair
            {
                condition = new RelativeTileTypeCondition<FloorDescriptor>
                {
                    predicate = descriptor => descriptor.FloorLocation == FloorDescriptor.Location.HALLWAY
                },
                generators = new TileGenerator[]
                {
                    new TileGenerator
                    {
                        source = new RandomTile(floorSettings.hallTiles.AsCollection()),
                        layer = floorSettings.tilemap
                    }
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
                generators = new TileGenerator[]
                {
                    new TileGenerator
                    {
                        source = new SingleTile(perimeterSettings.collisionTile),
                        layer = perimeterSettings.tilemap
                    }
                }
            },

            // Wall tiles

            new TileRulePair
            {
                // South center wall pieces have a top and bottom part, and aren't adjacent
                // to any floors diagonally southwards
                condition = new CompositeTileCondition(
                    new RelativeTileTypeCondition<FloorDescriptor>(),
                    new CompositeTileCondition(Operator.OR,
                        new RelativeTileTypeCondition<FloorDescriptor> { offset = Vector2Int.left },
                        new RelativeTileTypeCondition<FloorDescriptor> { offset = Vector2Int.right }
                    ),
                    new NegatedTileCondition(
                        new RelativeTileTypeCondition<FloorDescriptor> { offset = Vector2Int.down }
                    ),
                    new NegatedTileCondition(
                        new CompositeTileCondition(Operator.OR,
                            new RelativeTileTypeCondition<FloorDescriptor> { offset = new Vector2Int(1, -1) },
                            new RelativeTileTypeCondition<FloorDescriptor> { offset = new Vector2Int(-1, -1) }
                        )
                    )
                ),
                generators = new TileGenerator[]
                {
                    new TileGenerator
                    {
                        source = new SingleTile(wallSettings.southCenterUpper),
                        layer = wallSettings.foregroundTilemap
                    },
                    new TileGenerator
                    {
                        offset = Vector2Int.down,
                        source = new SingleTile(wallSettings.southCenterLower),
                        layer = wallSettings.foregroundTilemap
                    }
                }
            },

                        new TileRulePair
            {
                // North center wall is not on a floor, has floor south,
                // and no floor east or west
                condition = new CompositeTileCondition(
                    new RelativeTileTypeCondition<FloorDescriptor> { offset = Vector2Int.down },
                    new NegatedTileCondition(
                        new CompositeTileCondition(Operator.OR,
                            new RelativeTileTypeCondition<FloorDescriptor>(),
                            new RelativeTileTypeCondition<FloorDescriptor> { offset = Vector2Int.left },
                            new RelativeTileTypeCondition<FloorDescriptor> { offset = Vector2Int.right }
                        )
                    )
                ),
                generators = new TileGenerator[]
                {
                    new TileGenerator
                    {
                        source = new SingleTile(wallSettings.northCenter),
                        layer = wallSettings.backgroundTilemap
                    },
                }
            },

            new TileRulePair
            {
                // South-east reflexive corner has a top and bottom part, and has
                // floor east and west, but not south or south-east
                condition = new CompositeTileCondition(
                    new RelativeTileTypeCondition<FloorDescriptor>(),
                    new RelativeTileTypeCondition<FloorDescriptor> { offset = Vector2Int.left },
                    new RelativeTileTypeCondition<FloorDescriptor> { offset = Vector2Int.right },
                    new RelativeTileTypeCondition<FloorDescriptor> { offset = new Vector2Int(-1, -1) },
                    new NegatedTileCondition(
                        new CompositeTileCondition(Operator.OR,
                            new RelativeTileTypeCondition<FloorDescriptor> { offset = Vector2Int.down },
                            new RelativeTileTypeCondition<FloorDescriptor> { offset = new Vector2Int(1, -1) }
                        )
                    )
                ),
                generators = new TileGenerator[]
                {
                    new TileGenerator
                    {
                        source = new SingleTile(wallSettings.southEastReflexUpper),
                        layer = wallSettings.foregroundTilemap
                    },
                    new TileGenerator
                    {
                        offset = Vector2Int.down,
                        source = new SingleTile(wallSettings.southEastReflexLower),
                        layer = wallSettings.foregroundTilemap
                    }
                }
            },

            new TileRulePair
            {
                // South-west reflexive corner has a top and bottom part, and has
                // floor east and west, but not south or south-west
                condition = new CompositeTileCondition(
                    new RelativeTileTypeCondition<FloorDescriptor>(),
                    new RelativeTileTypeCondition<FloorDescriptor> { offset = Vector2Int.left },
                    new RelativeTileTypeCondition<FloorDescriptor> { offset = Vector2Int.right },
                    new RelativeTileTypeCondition<FloorDescriptor> { offset = new Vector2Int(1, -1) },
                    new NegatedTileCondition(
                        new CompositeTileCondition(Operator.OR,
                            new RelativeTileTypeCondition<FloorDescriptor> { offset = Vector2Int.down },
                            new RelativeTileTypeCondition<FloorDescriptor> { offset = new Vector2Int(-1, -1) }
                        )
                    )
                ),
                generators = new TileGenerator[]
                {
                    new TileGenerator
                    {
                        source = new SingleTile(wallSettings.southWestReflexUpper),
                        layer = wallSettings.foregroundTilemap
                    },
                    new TileGenerator
                    {
                        offset = Vector2Int.down,
                        source = new SingleTile(wallSettings.southWestReflexLower),
                        layer = wallSettings.foregroundTilemap
                    }
                }
            },

            new TileRulePair
            {
                // East center wall piece is not on a floor, has no floor north or
                // south, but has a floor west and south-west
                condition = new CompositeTileCondition(
                    new RelativeTileTypeCondition<FloorDescriptor> { offset = Vector2Int.left },
                    new RelativeTileTypeCondition<FloorDescriptor> { offset = new Vector2Int(-1, -1) },
                    new NegatedTileCondition(
                        new CompositeTileCondition(Operator.OR,
                            new RelativeTileTypeCondition<FloorDescriptor>(),
                            new RelativeTileTypeCondition<FloorDescriptor> { offset = Vector2Int.down },
                            new RelativeTileTypeCondition<FloorDescriptor> { offset = Vector2Int.up }
                        )
                    )
                ),
                generators = new TileGenerator[]
                {
                    new TileGenerator
                    {
                        source = new SingleTile(wallSettings.eastCenter),
                        layer = wallSettings.foregroundTilemap
                    },
                }
            },

            new TileRulePair
            {
                // West center wall piece is not on a floor, has no floor north or
                // south, but has a floor east and south-east
                condition = new CompositeTileCondition(
                    new RelativeTileTypeCondition<FloorDescriptor> { offset = Vector2Int.right },
                    new RelativeTileTypeCondition<FloorDescriptor> { offset = new Vector2Int(1, -1) },
                    new NegatedTileCondition(
                        new CompositeTileCondition(Operator.OR,
                            new RelativeTileTypeCondition<FloorDescriptor>(),
                            new RelativeTileTypeCondition<FloorDescriptor> { offset = Vector2Int.down },
                            new RelativeTileTypeCondition<FloorDescriptor> { offset = Vector2Int.up }
                        )
                    )
                ),
                generators = new TileGenerator[]
                {
                    new TileGenerator
                    {
                        source = new SingleTile(wallSettings.westCenter),
                        layer = wallSettings.foregroundTilemap
                    },
                }
            },

            new TileRulePair
            {
                // North-east corner is not on a floor, has no floor west or
                // south, but has floor south-west 
                condition = new CompositeTileCondition(
                    new RelativeTileTypeCondition<FloorDescriptor> { offset = new Vector2Int(-1, -1) },
                    new NegatedTileCondition(
                        new CompositeTileCondition(Operator.OR,
                            new RelativeTileTypeCondition<FloorDescriptor>(),
                            new RelativeTileTypeCondition<FloorDescriptor> { offset = Vector2Int.down },
                            new RelativeTileTypeCondition<FloorDescriptor> { offset = Vector2Int.left }
                        )
                    )
                ),
                generators = new TileGenerator[]
                {
                    new TileGenerator
                    {
                        source = new SingleTile(wallSettings.northEast),
                        layer = wallSettings.backgroundTilemap
                    },
                }
            },

            new TileRulePair
            {
                // North-west corner is not on a floor, has no floor east or
                // south, but has floor south-east 
                condition = new CompositeTileCondition(
                    new RelativeTileTypeCondition<FloorDescriptor> { offset = new Vector2Int(1, -1) },
                    new NegatedTileCondition(
                        new CompositeTileCondition(Operator.OR,
                            new RelativeTileTypeCondition<FloorDescriptor>(),
                            new RelativeTileTypeCondition<FloorDescriptor> { offset = Vector2Int.down },
                            new RelativeTileTypeCondition<FloorDescriptor> { offset = Vector2Int.right }
                        )
                    )
                ),
                generators = new TileGenerator[]
                {
                    new TileGenerator
                    {
                        source = new SingleTile(wallSettings.northWest),
                        layer = wallSettings.backgroundTilemap
                    },
                }
            },

            new TileRulePair
            {
                // North-east reflex corner is not on a floor, has floor west
                // and south, and no floor east or north
                condition = new CompositeTileCondition(
                    new RelativeTileTypeCondition<FloorDescriptor> { offset = Vector2Int.left },
                    new RelativeTileTypeCondition<FloorDescriptor> { offset = Vector2Int.down },
                    new NegatedTileCondition(
                        new CompositeTileCondition(Operator.OR,
                            new RelativeTileTypeCondition<FloorDescriptor>(),
                            new RelativeTileTypeCondition<FloorDescriptor> { offset = Vector2Int.up },
                            new RelativeTileTypeCondition<FloorDescriptor> { offset = Vector2Int.right }
                        )
                    )
                ),
                generators = new TileGenerator[]
                {
                    new TileGenerator
                    {
                        source = new SingleTile(wallSettings.northEastReflex),
                        layer = wallSettings.backgroundTilemap
                    },
                }
            },

            new TileRulePair
            {
                // North-west reflex corner is not on a floor, has floor east
                // and south, and no floor west or north
                condition = new CompositeTileCondition(
                    new RelativeTileTypeCondition<FloorDescriptor> { offset = Vector2Int.right },
                    new RelativeTileTypeCondition<FloorDescriptor> { offset = Vector2Int.down },
                    new NegatedTileCondition(
                        new CompositeTileCondition(Operator.OR,
                            new RelativeTileTypeCondition<FloorDescriptor>(),
                            new RelativeTileTypeCondition<FloorDescriptor> { offset = Vector2Int.up },
                            new RelativeTileTypeCondition<FloorDescriptor> { offset = Vector2Int.left }
                        )
                    )
                ),
                generators = new TileGenerator[]
                {
                    new TileGenerator
                    {
                        source = new SingleTile(wallSettings.northWestReflex),
                        layer = wallSettings.backgroundTilemap
                    },
                }
            },

            new TileRulePair
            {
                // South-east corner has an upper and lower part, is not on a floor, has
                // floor west, and no floor south-west or south
                condition = new CompositeTileCondition(
                    new RelativeTileTypeCondition<FloorDescriptor> { offset = Vector2Int.left },
                    new NegatedTileCondition(
                        new CompositeTileCondition(Operator.OR,
                            new RelativeTileTypeCondition<FloorDescriptor>(),
                            new RelativeTileTypeCondition<FloorDescriptor> { offset = Vector2Int.down },
                            new RelativeTileTypeCondition<FloorDescriptor> { offset = new Vector2Int(-1, -1) }
                        )
                    )
                ),
                generators = new TileGenerator[]
                {
                    new TileGenerator
                    {
                        source = new SingleTile(wallSettings.southEastUpper),
                        layer = wallSettings.foregroundTilemap
                    },
                    new TileGenerator
                    {
                        offset = Vector2Int.down,
                        source = new SingleTile(wallSettings.southEastLower),
                        layer = wallSettings.foregroundTilemap
                    },
                }
            },

            new TileRulePair
            {
                // South-west corner has an upper and lower part, is not on a floor, has
                // floor east, and no floor south-east or south
                condition = new CompositeTileCondition(
                    new RelativeTileTypeCondition<FloorDescriptor> { offset = Vector2Int.right },
                    new NegatedTileCondition(
                        new CompositeTileCondition(Operator.OR,
                            new RelativeTileTypeCondition<FloorDescriptor>(),
                            new RelativeTileTypeCondition<FloorDescriptor> { offset = Vector2Int.down },
                            new RelativeTileTypeCondition<FloorDescriptor> { offset = new Vector2Int(1, -1) }
                        )
                    )
                ),
                generators = new TileGenerator[]
                {
                    new TileGenerator
                    {
                        source = new SingleTile(wallSettings.southWestUpper),
                        layer = wallSettings.foregroundTilemap
                    },
                    new TileGenerator
                    {
                        offset = Vector2Int.down,
                        source = new SingleTile(wallSettings.southWestLower),
                        layer = wallSettings.foregroundTilemap
                    },
                }
            },
        };
    }
}
