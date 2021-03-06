using System;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

public static class WorldGenerator
{
    [System.Serializable]
    public class Parameters
    {
        [SerializeField]
        [Min(1)]
        [Tooltip("The width of the hallways")]
        public int HallwayWidth = 1;

        [SerializeField]
        [Tooltip("Size of the world, in terms of hallway widths")]
        public Vector2Int WorldScale = Vector2Int.one * 16;

        [SerializeField]
        [Tooltip("Minimum size of each sector that makes up rooms, as a multiple of hallway widths")]
        public Vector2Int MinimumCellularSectorSize = Vector2Int.one * 16;

        [SerializeField]
        [Tooltip("Maximum size of each sector that makes up rooms, as a multiple of hallway widths")]
        public Vector2Int MaximumCellularSectorSize = Vector2Int.one * 8;

        [SerializeField]
        [Min(0.25f)]
        [Tooltip("Maximum room size, as a multiple of the maximum sector size")]
        public float MaximumRoomSizeScale = 2f;

        [SerializeField]
        [Min(1)]
        [Tooltip("The minimum width of same-room sector overlaps")]
        public int MinimumSectorOverlapWidth = 4;

        [SerializeField]
        [Min(1)]
        [Tooltip("Determines how rectangular rooms will be")]
        public int MaximumSectionsPerRoom = 4;

        [SerializeField]
        [Range(0f, 1f)]
        [Tooltip("Determines the number of extra hallways generated")]
        public float BonusHallwaySpawnChance = 0.1f;

        [SerializeField]
        [Min(0)]
        [Tooltip("Determines the maximum number of bonus hallways generated")]
        public int MaximumBonusHallways = 4;

        [SerializeField]
        [Tooltip("How densely populated the world will be with rooms")]
        public UnitScalar RoomDensity = 0.5f;
        [SerializeField]
        [Tooltip("Skews sector sizes to be smaller as room generation progresses")]
        public AnimationCurve SectorSizeDecay = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        public Vector2Int CellularDimensions => WorldScale * (HallwayWidth + 1) + Vector2Int.one;

        public Parameters(
            int hallwayWidth,
            Vector2Int worldScale,
            Vector2Int minimumCellularSectorSize,
            Vector2Int maximumCellularSectorSize,
            float maximumRoomSizeScale,
            int minimumSectorOverlapWidth,
            int maximumSectionsPerRoom,
            UnitScalar roomDensity,
            AnimationCurve sectorSizeDecay
            )
        {
            HallwayWidth = hallwayWidth;
            WorldScale = worldScale;
            MinimumCellularSectorSize = minimumCellularSectorSize;
            MaximumCellularSectorSize = maximumCellularSectorSize;
            MaximumRoomSizeScale = maximumRoomSizeScale;
            MinimumSectorOverlapWidth = minimumSectorOverlapWidth;
            MaximumSectionsPerRoom = maximumSectionsPerRoom;
            RoomDensity = roomDensity;
            SectorSizeDecay = sectorSizeDecay;
        }
    }

    private static class RandomUtils
    {
        public static int RandomOddInRange(int min, int max)
        {
            return Random.Range(min / 2, max / 2) * 2 + 1;
        }
    }

    private static class RoomGenerator
    {
        private static RangeInt VALID_CONSECUTIVE_FAILED_ROOM_INSERTION_ATTEMPTS = new RangeInt(1, 64);

        private static bool CanMergeRooms(IEnumerable<GeneratedRoom> rooms, RectInt section, Parameters parameters)
        {
            var numSections = rooms.Aggregate(0, (a, r) => a + r.SectionCount) + 1;
            var numCells = rooms.Aggregate(0, (a, r) => a + r.SizeMerging(section));
            var minMergeWidth = rooms.Min(r => r.OpeningWidthMerging(section));

            return numSections <= parameters.MaximumSectionsPerRoom &&
                numCells < Mathf.FloorToInt(parameters.MaximumCellularSectorSize.x * parameters.MaximumCellularSectorSize.y * parameters.MaximumRoomSizeScale) &&
                minMergeWidth >= parameters.MinimumSectorOverlapWidth;
        }

        private static bool TryInsertSection(
            RectInt section,
            IDictionary<Vector2Int, GeneratedRoom> lookup,
            Parameters parameters,
            out GeneratedRoom updatedRoom)
        {
            // Get all the rooms this new section would overlap with
            HashSet<GeneratedRoom> overlappingRooms = new HashSet<GeneratedRoom>();
            foreach (var pos in section.allPositionsWithin)
                if (lookup.TryGetValue(pos, out var room))
                    overlappingRooms.Add(room);

            if (overlappingRooms.Count == 0)
            {
                // No overlapping rooms, just create a new one
                updatedRoom = new GeneratedRoom(section);
                return true;
            }
            else if (CanMergeRooms(overlappingRooms, section, parameters))
            {
                GeneratedRoom newRoom = new GeneratedRoom(section);
                foreach (var room in overlappingRooms)
                {
                    foreach (var sectionToMove in room.GetSections())
                    {
                        newRoom.AddSection(sectionToMove);
                        foreach (var pos in sectionToMove.allPositionsWithin)
                        {
                            lookup[pos] = newRoom;
                        }
                    }
                }

                updatedRoom = newRoom;
                return true;
            }
            else
            {
                updatedRoom = default;
                return false;
            }
        }

        public static GeneratedRoomMapping GenerateRooms(Parameters parameters)
        {
            Dictionary<Vector2Int, GeneratedRoom> roomLookup = new Dictionary<Vector2Int, GeneratedRoom>();

            int consecutiveFailedAttempts = 0;
            int maxConsecutiveFailedAttempts = VALID_CONSECUTIVE_FAILED_ROOM_INSERTION_ATTEMPTS.Resolve(parameters.RoomDensity);

            while (consecutiveFailedAttempts < maxConsecutiveFailedAttempts)
            {
                float t = parameters.SectorSizeDecay.Evaluate(consecutiveFailedAttempts / (float)maxConsecutiveFailedAttempts);
                int minSectWidth = parameters.MinimumCellularSectorSize.x;
                float maxSectWidth = Mathf.Lerp(parameters.MaximumCellularSectorSize.x, parameters.MinimumCellularSectorSize.x, t);
                int minSectHeight = parameters.MinimumCellularSectorSize.y;
                float maxSectHeight = Mathf.Lerp(parameters.MaximumCellularSectorSize.y, parameters.MinimumCellularSectorSize.y, t);

                int width = RandomUtils.RandomOddInRange(minSectWidth, Mathf.CeilToInt(maxSectWidth));
                int height = RandomUtils.RandomOddInRange(minSectHeight, Mathf.CeilToInt(maxSectHeight));

                int x = RandomUtils.RandomOddInRange(1, parameters.CellularDimensions.x - width);
                int y = RandomUtils.RandomOddInRange(1, parameters.CellularDimensions.y - height);

                // Offset allowing a hallways around the perimiter
                x += 2;
                y += 2;

                RectInt section = new RectInt(x, y, width, height);

                if (TryInsertSection(section, roomLookup, parameters, out var room))
                {
                    foreach (var cell in section.allPositionsWithin)
                        roomLookup[cell] = room;

                    consecutiveFailedAttempts = 0;
                }
                else
                {
                    consecutiveFailedAttempts++;
                }
            }

            var allRooms = new HashSet<GeneratedRoom>(roomLookup.Values);

            return new GeneratedRoomMapping(new List<GeneratedRoom>(allRooms), roomLookup);
        }
    }

    private static class HallwayGenerator
    {
        private static Vector2Int[] DIRECTIONS = new Vector2Int[]
        {
        Vector2Int.up,
        Vector2Int.right,
        Vector2Int.down,
        Vector2Int.left,
        };

        public static List<GeneratedHallway> GenerateHallways(GeneratedRoomMapping rooms, Parameters parameters)
        {
            // Total generated maze size is 4 units bigger per dimension, giving
            // us a 2-width wide border around the normal world size. We place
            // hallways on odd-squares, so this would put halls on the inner border.
            // With 1 cell of padding (again due to the odd number rule) for room
            // generation, this will let us put halls around the edge of the map.
            Vector2Int gridSize = parameters.CellularDimensions + Vector2Int.one * 4;

            // Generate a spanning maze
            bool[,] maze = GenerateMaze(gridSize);

            // Filter the maze, isolating cells that form pathways that lead between
            // rooms and don't overlap rooms, also determing door cells
            List<GeneratedHallway> validHallways = GetConnectingHallways(maze, rooms);

            // Filter the hallways futher, removing redundant hallways and removing
            // dead ends
            List<GeneratedHallway> reducedHallways = ReduceHallways(validHallways, parameters);

            return reducedHallways;
        }

        private static List<GeneratedHallway> ReduceHallways(List<GeneratedHallway> validHallways, Parameters parameters)
        {
            HashSet<Vector2Int> SearchPath(Vector2Int start, Vector2Int end, ISet<Vector2Int> path)
            {
                Queue<List<Vector2Int>> candidates = new Queue<List<Vector2Int>>();
                candidates.Enqueue(new List<Vector2Int>(new Vector2Int[] { start }));

                while (candidates.Count > 0)
                {
                    var walk = candidates.Dequeue();
                    var lastStep = walk[walk.Count - 1];
                    if (lastStep == end)
                        return new HashSet<Vector2Int>(walk);

                    var neighbors = DIRECTIONS
                        .Select(d => lastStep + d)
                        .Where(c => path.Contains(c));

                    foreach (var neighbor in neighbors)
                    {
                        // Don't double back
                        if (walk.Count > 1 && walk[walk.Count - 2] == neighbor)
                            continue;

                        var newPath = new List<Vector2Int>(walk);
                        newPath.Add(neighbor);
                        candidates.Enqueue(newPath);
                    }
                }

                Assert.IsTrue(false, "Unable to find a subpath between doors");

                return new HashSet<Vector2Int>();
            }

            GeneratedHallway GetSimplifiedHallway(GeneratedHallway hallway)
            {
                if (hallway.Path.Count == 1)
                    return hallway;

                var someDoor = hallway.DoorMapping.First(_ => true);
                HashSet<Vector2Int> newPath = new HashSet<Vector2Int>();

                foreach (var destDoor in hallway.DoorMapping.Where(d => d.Value != someDoor.Value))
                    newPath.UnionWith(SearchPath(someDoor.Value, destDoor.Value, hallway.Path));

                return new GeneratedHallway(newPath, hallway.DoorMapping);
            }

            List<GeneratedHallway> newHallways = new List<GeneratedHallway>();
            HashSet<GeneratedRoom> remainingRooms = new HashSet<GeneratedRoom>();
            HashSet<GeneratedHallway> pickedHallwayOriginals = new HashSet<GeneratedHallway>();
            foreach (var room in validHallways.SelectMany(h => h.DoorMapping.Keys))
                remainingRooms.Add(room);

            // First remove a random room. This will seed the connected room set
            remainingRooms.Remove(remainingRooms.First(_ => true));

            // Iterate until we've connected every room with a hallway
            while (remainingRooms.Count > 0)
            {
                // Get all the hallways that will connect a connected room to a disconnected room
                var fringeHallways = validHallways
                    .Where(h => h.DoorMapping.Keys.Any(r => !remainingRooms.Contains(r)))
                    .Where(h => h.DoorMapping.Keys.Any(r => remainingRooms.Contains(r)))
                    .ToArray();

                // Pick a random hallway from that list
                var selectedHallway = fringeHallways[Random.Range(0, fringeHallways.Length)];

                // Remove every room the hallway connects from the disconnected room set
                IEnumerable<GeneratedRoom> connectedRooms = selectedHallway.DoorMapping.Keys;
                foreach (var room in connectedRooms.Where(remainingRooms.Contains))
                    remainingRooms.Remove(room);

                // Finally, add the simplified (dead-ends removed) hallway
                newHallways.Add(GetSimplifiedHallway(selectedHallway));

                // Also keep track of the original hallway we chose
                pickedHallwayOriginals.Add(selectedHallway);
            }

            int numBonusHallways = 0;

            // Add in some bonus hallways
            foreach (var bonusHallway in validHallways.Where(h => !pickedHallwayOriginals.Contains(h)))
            {
                if (Random.value < parameters.BonusHallwaySpawnChance && numBonusHallways < parameters.MaximumBonusHallways)
                {
                    newHallways.Add(GetSimplifiedHallway(bonusHallway));
                    numBonusHallways++;
                }
            }

            return newHallways;
        }

        private static List<GeneratedHallway> GetConnectingHallways(bool[,] maze, GeneratedRoomMapping rooms)
        {
            Vector2Int gridSize = new Vector2Int(maze.GetLength(1), maze.GetLength(0));

            bool IsRoom(Vector2Int pos) => rooms.Layout.ContainsKey(pos);
            bool IsValid(Vector2Int pos) =>
                pos.x >= 0 && pos.x < gridSize.x && pos.y > 0 && pos.y < gridSize.y;

            IEnumerable<Vector2Int> ExpandHallway(Vector2Int startingPos)
            {
                HashSet<Vector2Int> walkedPositions = new HashSet<Vector2Int>();
                Queue<Vector2Int> fringe = new Queue<Vector2Int>(new Vector2Int[] { startingPos });

                while (fringe.Count > 0)
                {
                    var current = fringe.Dequeue();
                    walkedPositions.Add(current);

                    foreach (var dir in DIRECTIONS)
                    {
                        var newPos = current + dir;

                        if (IsValid(newPos) && maze[newPos.y, newPos.x] && !walkedPositions.Contains(newPos) && !IsRoom(newPos))
                            fringe.Enqueue(newPos);
                    }

                    yield return current;
                }
            }

            bool[,] colors = new bool[maze.GetLength(0), maze.GetLength(1)];
            List<GeneratedHallway> validHallways = new List<GeneratedHallway>();

            for (int y = 0; y < maze.GetLength(0); y++)
            {
                for (int x = 0; x < maze.GetLength(1); x++)
                {
                    var position = new Vector2Int(x, y);

                    // Skip if either a wall, we've traced this path before, or it's behind a room
                    if (!maze[y, x] || colors[y, x] || IsRoom(position))
                        continue;

                    // New path: Track paths to connecting rooms
                    var path = new HashSet<Vector2Int>(ExpandHallway(position));

                    var doorMapping = path
                        .SelectMany(pathPos => DIRECTIONS.Select(d => (from: pathPos, to: pathPos + d)))
                        .Where(x => IsValid(x.to) && maze[x.to.y, x.to.x] && IsRoom(x.to))
                        .Select(n => (entrance: n.from, room: rooms.Layout[n.to]))
                        .ToList();

                    Dictionary<GeneratedRoom, Vector2Int> entrances = new Dictionary<GeneratedRoom, Vector2Int>();
                    foreach (var (entrance, room) in doorMapping)
                        entrances[room] = entrance;

                    if (entrances.Count > 1)
                        validHallways.Add(new GeneratedHallway(path, entrances));

                    foreach (var pathPos in path)
                        colors[pathPos.y, pathPos.x] = true;
                }
            }

            return validHallways;
        }

        private static bool[,] GenerateMaze(Vector2Int gridSize)
        {
            bool[,] maze = new bool[gridSize.y, gridSize.x];

            IEnumerable<Vector2Int> GetNeighbors(Vector2Int pos, bool type) => DIRECTIONS
                .Select(dir => pos + dir * 2)
                .Where(c => c.x >= 0 && c.x < gridSize.x)
                .Where(c => c.y >= 0 && c.y < gridSize.y)
                .Where(f => maze[f.y, f.x] == type);

            T TakeRandom<T>(IList<T> list)
            {
                int idx = Random.Range(0, list.Count);
                var elem = list[idx];
                list[idx] = list[list.Count - 1];
                list.RemoveAt(list.Count - 1);
                return elem;
            }

            Vector2Int startingCell = new Vector2Int(
                RandomUtils.RandomOddInRange(0, gridSize.x),
                RandomUtils.RandomOddInRange(0, gridSize.y)
            );

            maze[startingCell.y, startingCell.x] = true;
            List<Vector2Int> pool = new List<Vector2Int>(GetNeighbors(startingCell, false));

            while (pool.Count > 0)
            {
                var currentCell = TakeRandom(pool);
                if (maze[currentCell.y, currentCell.x])
                    continue;

                maze[currentCell.y, currentCell.x] = true;

                var neighbors = GetNeighbors(currentCell, true).ToList();
                var randomNeighbor = TakeRandom(neighbors);
                var middleCell = (currentCell + randomNeighbor) / 2;

                maze[middleCell.y, middleCell.x] = true;

                pool.AddRange(GetNeighbors(currentCell, false));
            }

            return maze;
        }
    }

    private static class AirlockGenerator
    {
        public static readonly Vector2Int[] DIAGONALS = new Vector2Int[]
        {
            new Vector2Int(1, 1),
            new Vector2Int(1, -1),
            new Vector2Int(-1, 1),
            new Vector2Int(-1, -1),
        };

        public static readonly Vector2Int[] ORTHOGONALS = new Vector2Int[]
        {
            Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
        };

        public static IReadOnlyList<GeneratedAirlock> GenerateAirlocks(GeneratedRoomMapping roomLayout, IReadOnlyList<GeneratedHallway> hallways, Parameters parameters)
        {
            HashSet<Vector2Int> allHallwaySquares = new HashSet<Vector2Int>(hallways.SelectMany(h => h.Path));

            bool IsRoomInDirection(Vector2Int cell, Vector2Int direction) =>
                roomLayout.Layout.ContainsKey(cell + direction);

            bool IsHallwayInDirection(Vector2Int cell, Vector2Int direction) =>
                allHallwaySquares.Contains(cell + direction);

            var allAirlocks = new List<GeneratedAirlock>();
            for (int x = -2; x < parameters.CellularDimensions.x + 2; x += 2)
            {
                for (int y = -2; y < parameters.CellularDimensions.y + 2; y += 2)
                {
                    var cell = new Vector2Int(x, y);

                    // If it's a room or hallway, it's no good
                    if (IsRoomInDirection(cell, Vector2Int.zero) || IsHallwayInDirection(cell, Vector2Int.zero))
                        continue;

                    // If a diagnonal or an orthogonal is a hallway, it's no good
                    if (Enumerable.Concat(DIAGONALS, ORTHOGONALS).Any(d => IsHallwayInDirection(cell, d)))
                        continue;

                    // It must be orthogonal to exactly one room
                    if (ORTHOGONALS.Count(o => IsRoomInDirection(cell, o)) != 1)
                        continue;

                    var dir = ORTHOGONALS.First(o => IsRoomInDirection(cell, o));
                    var room = roomLayout.Layout[cell + dir];
                    var neighboringCellsInRooms = Enumerable.Concat(ORTHOGONALS, DIAGONALS)
                        .Select(d => cell + d)
                        .Where(c => roomLayout.Layout.ContainsKey(c));

                    // All neighboring rooms must be the same room
                    if (neighboringCellsInRooms.Select(c => roomLayout.Layout[c]).Distinct().Count() > 1)
                        continue;

                    // Either the room aligned cells must be aligned on the x or y axis
                    if (neighboringCellsInRooms.Select(c => c.x).Distinct().Count() != 1 &&
                        neighboringCellsInRooms.Select(c => c.y).Distinct().Count() != 1)
                    {
                        continue;
                    }

                    allAirlocks.Add(new GeneratedAirlock(room, cell, dir));
                }
            }

            return allAirlocks;
        }
    }

    public static CellMapping Generate(Parameters parameters)
    {
        return Profile.Debug("Generate world layout", () =>
        {
            var rooms = Profile.Debug("Generate room layout", () => RoomGenerator.GenerateRooms(parameters));
            var hallways = Profile.Debug("Generate hallway layout", () => HallwayGenerator.GenerateHallways(rooms, parameters));
            var airlocks = Profile.Debug("Generate airlock layout", () => AirlockGenerator.GenerateAirlocks(rooms, hallways, parameters));
            return new CellMapping(rooms, hallways, airlocks, parameters);
        });
    }
}
