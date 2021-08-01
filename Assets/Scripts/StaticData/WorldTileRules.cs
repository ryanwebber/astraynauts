using System;
using System.Collections.Generic;
using UnityEngine;
using static WorldLoader;

public static class WorldTileRules
{
    public static ICollection<TileRule> GetRules(
        FloorSettings floorSettings,
        PerimeterSettings perimeterSettings,
        WallSettings wallSettings)
    {
        return new TileRule[]
        {
            // Room tiles

            new TileRule
            {
                condition = new RelativeTileTypeCondition<FloorDescriptor> {
                    predicate = FloorDescriptor.IsA(FloorDescriptor.Location.ROOM)
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

            new TileRule
            {
                condition = new RelativeTileTypeCondition<FloorDescriptor>
                {
                    predicate = FloorDescriptor.IsA(FloorDescriptor.Location.HALLWAY)
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

            // Portal tiles
            new TileRule
            {
                condition = new AllOfCondition(
                    new RelativeTileTypeCondition<FloorDescriptor>
                    {
                        predicate = FloorDescriptor.IsA(FloorDescriptor.Location.TELEPORTER)
                    },
                    new RelativeTileTypeCondition<FloorDescriptor>
                    {
                        offset = Vector2Int.right,
                        predicate = FloorDescriptor.IsA(FloorDescriptor.Location.TELEPORTER)
                    },
                    new RelativeTileTypeCondition<FloorDescriptor>
                    {
                        offset = Vector2Int.down,
                        predicate = FloorDescriptor.IsA(FloorDescriptor.Location.TELEPORTER)
                    }
                ),
                generators = new TileGenerator[]
                {
                    new TileGenerator
                    {
                        source = new SingleTile(floorSettings.portalNorthWest),
                        layer = floorSettings.tilemap,
                        offset = Vector2Int.zero
                    },
                    new TileGenerator
                    {
                        source = new SingleTile(floorSettings.portalNorthEast),
                        layer = floorSettings.tilemap,
                        offset = Vector2Int.right
                    },
                    new TileGenerator
                    {
                        source = new SingleTile(floorSettings.portalSouthWest),
                        layer = floorSettings.tilemap,
                        offset = Vector2Int.down
                    },
                    new TileGenerator
                    {
                        source = new SingleTile(floorSettings.portalSouthEast),
                        layer = floorSettings.tilemap,
                        offset = Vector2Int.right + Vector2Int.down
                    },
                }
            },

            // Perimiter tiles

            new TileRule
            {
                condition = new AllOfCondition(
                    new NoneOfCondition(
                        new RelativeTileTypeCondition<FloorDescriptor>()
                    ),
                    new AnyOfCondition(
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

            new TileRule
            {
                // South center wall pieces have a top and bottom part, and aren't adjacent
                // to any floors diagonally southwards
                condition = new AllOfCondition(
                    new RelativeTileTypeCondition<FloorDescriptor>(),
                    new AnyOfCondition(
                        new RelativeTileTypeCondition<FloorDescriptor> { offset = Vector2Int.left },
                        new RelativeTileTypeCondition<FloorDescriptor> { offset = Vector2Int.right }
                    ),
                    new NoneOfCondition(
                        new RelativeTileTypeCondition<FloorDescriptor> { offset = Vector2Int.down },
                        new RelativeTileTypeCondition<FloorDescriptor> { offset = new Vector2Int(1, -1) },
                        new RelativeTileTypeCondition<FloorDescriptor> { offset = new Vector2Int(-1, -1) }
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

            new TileRule
            {
                // North center wall is not on a floor, has floor south,
                // and no floor east or west
                condition = new AllOfCondition(
                    new RelativeTileTypeCondition<FloorDescriptor> { offset = Vector2Int.down },
                    new NoneOfCondition(
                        new RelativeTileTypeCondition<FloorDescriptor>(),
                        new RelativeTileTypeCondition<FloorDescriptor> { offset = Vector2Int.left },
                        new RelativeTileTypeCondition<FloorDescriptor> { offset = Vector2Int.right }
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

            new TileRule
            {
                // South-east reflexive corner has a top and bottom part, and has
                // floor east and west, but not south or south-east
                condition = new AllOfCondition(
                    new RelativeTileTypeCondition<FloorDescriptor>(),
                    new RelativeTileTypeCondition<FloorDescriptor> { offset = Vector2Int.left },
                    new RelativeTileTypeCondition<FloorDescriptor> { offset = Vector2Int.right },
                    new RelativeTileTypeCondition<FloorDescriptor> { offset = new Vector2Int(-1, -1) },
                    new NoneOfCondition(
                        new RelativeTileTypeCondition<FloorDescriptor> { offset = Vector2Int.down },
                        new RelativeTileTypeCondition<FloorDescriptor> { offset = new Vector2Int(1, -1) }
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

            new TileRule
            {
                // South-west reflexive corner has a top and bottom part, and has
                // floor east and west, but not south or south-west
                condition = new AllOfCondition(
                    new RelativeTileTypeCondition<FloorDescriptor>(),
                    new RelativeTileTypeCondition<FloorDescriptor> { offset = Vector2Int.left },
                    new RelativeTileTypeCondition<FloorDescriptor> { offset = Vector2Int.right },
                    new RelativeTileTypeCondition<FloorDescriptor> { offset = new Vector2Int(1, -1) },
                    new NoneOfCondition(
                        new RelativeTileTypeCondition<FloorDescriptor> { offset = Vector2Int.down },
                        new RelativeTileTypeCondition<FloorDescriptor> { offset = new Vector2Int(-1, -1) }
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

            new TileRule
            {
                // East center wall piece is not on a floor, has no floor north or
                // south, but has a floor west and south-west
                condition = new AllOfCondition(
                    new RelativeTileTypeCondition<FloorDescriptor> { offset = Vector2Int.left },
                    new RelativeTileTypeCondition<FloorDescriptor> { offset = new Vector2Int(-1, -1) },
                    new NoneOfCondition(
                        new RelativeTileTypeCondition<FloorDescriptor>(),
                        new RelativeTileTypeCondition<FloorDescriptor> { offset = Vector2Int.down },
                        new RelativeTileTypeCondition<FloorDescriptor> { offset = Vector2Int.up }
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

            new TileRule
            {
                // West center wall piece is not on a floor, has no floor north or
                // south, but has a floor east and south-east
                condition = new AllOfCondition(
                    new RelativeTileTypeCondition<FloorDescriptor> { offset = Vector2Int.right },
                    new RelativeTileTypeCondition<FloorDescriptor> { offset = new Vector2Int(1, -1) },
                    new NoneOfCondition(
                        new RelativeTileTypeCondition<FloorDescriptor>(),
                        new RelativeTileTypeCondition<FloorDescriptor> { offset = Vector2Int.down },
                        new RelativeTileTypeCondition<FloorDescriptor> { offset = Vector2Int.up }
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

            new TileRule
            {
                // North-east corner is not on a floor, has no floor west or
                // south, but has floor south-west 
                condition = new AllOfCondition(
                    new RelativeTileTypeCondition<FloorDescriptor> { offset = new Vector2Int(-1, -1) },
                    new NoneOfCondition(
                        new RelativeTileTypeCondition<FloorDescriptor>(),
                        new RelativeTileTypeCondition<FloorDescriptor> { offset = Vector2Int.down },
                        new RelativeTileTypeCondition<FloorDescriptor> { offset = Vector2Int.left }
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

            new TileRule
            {
                // North-west corner is not on a floor, has no floor east or
                // south, but has floor south-east 
                condition = new AllOfCondition(
                    new RelativeTileTypeCondition<FloorDescriptor> { offset = new Vector2Int(1, -1) },
                    new NoneOfCondition(
                        new RelativeTileTypeCondition<FloorDescriptor>(),
                        new RelativeTileTypeCondition<FloorDescriptor> { offset = Vector2Int.down },
                        new RelativeTileTypeCondition<FloorDescriptor> { offset = Vector2Int.right }
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

            new TileRule
            {
                // North-east reflex corner is not on a floor, has floor west
                // and south, and no floor east or north
                condition = new AllOfCondition(
                    new RelativeTileTypeCondition<FloorDescriptor> { offset = Vector2Int.left },
                    new RelativeTileTypeCondition<FloorDescriptor> { offset = Vector2Int.down },
                    new NoneOfCondition(
                        new RelativeTileTypeCondition<FloorDescriptor>(),
                        new RelativeTileTypeCondition<FloorDescriptor> { offset = Vector2Int.up },
                        new RelativeTileTypeCondition<FloorDescriptor> { offset = Vector2Int.right }
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

            new TileRule
            {
                // North-west reflex corner is not on a floor, has floor east
                // and south, and no floor west or north
                condition = new AllOfCondition(
                    new RelativeTileTypeCondition<FloorDescriptor> { offset = Vector2Int.right },
                    new RelativeTileTypeCondition<FloorDescriptor> { offset = Vector2Int.down },
                    new NoneOfCondition(
                        new RelativeTileTypeCondition<FloorDescriptor>(),
                        new RelativeTileTypeCondition<FloorDescriptor> { offset = Vector2Int.up },
                        new RelativeTileTypeCondition<FloorDescriptor> { offset = Vector2Int.left }
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

            new TileRule
            {
                // South-east corner has an upper and lower part, is not on a floor, has
                // floor west, and no floor south-west or south
                condition = new AllOfCondition(
                    new RelativeTileTypeCondition<FloorDescriptor> { offset = Vector2Int.left },
                    new NoneOfCondition(
                        new RelativeTileTypeCondition<FloorDescriptor>(),
                        new RelativeTileTypeCondition<FloorDescriptor> { offset = Vector2Int.down },
                        new RelativeTileTypeCondition<FloorDescriptor> { offset = new Vector2Int(-1, -1) }
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

            new TileRule
            {
                // South-west corner has an upper and lower part, is not on a floor, has
                // floor east, and no floor south-east or south
                condition = new AllOfCondition(
                    new RelativeTileTypeCondition<FloorDescriptor> { offset = Vector2Int.right },
                    new NoneOfCondition(
                        new RelativeTileTypeCondition<FloorDescriptor>(),
                        new RelativeTileTypeCondition<FloorDescriptor> { offset = Vector2Int.down },
                        new RelativeTileTypeCondition<FloorDescriptor> { offset = new Vector2Int(1, -1) }
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
