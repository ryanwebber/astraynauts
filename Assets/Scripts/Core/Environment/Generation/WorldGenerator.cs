﻿using System;
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
        [Tooltip("Determines how rectangular rooms will be.")]
        public int MaximumSectionsPerRoom = 4;

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

    public class WorldLayout
    {
        public readonly RoomLayout rooms;
        public readonly List<Hallway> hallways;

        public WorldLayout(RoomLayout rooms, List<Hallway> hallways)
        {
            this.rooms = rooms;
            this.hallways = hallways;
        }
    }

    public class Room
    {
        private List<RectInt> sections;

        public int CellCount => GetUniqueCells().Count;
        public int SectionCount => sections.Count;

        public Room(RectInt initialSection)
        {
            sections = new List<RectInt>(3);
            sections.Add(initialSection);
        }

        private HashSet<Vector2Int> GetUniqueCells()
        {
            HashSet<Vector2Int> allCells = new HashSet<Vector2Int>();
            foreach (var rect in sections)
                allCells.UnionWith(rect.GetAllPositions());

            return allCells;
        }

        public void AddSection(RectInt rect)
        {
            sections.Add(rect);
        }

        public IEnumerable<Vector2Int> GetAllCells()
        {
            return GetUniqueCells();
        }

        public IEnumerable<RectInt> GetSections()
        {
            return sections;
        }

        public RectInt GetSection(int idx)
        {
            return sections[idx];
        }

        public int SizeMerging(RectInt newSection)
        {
            var allCells = GetUniqueCells();
            allCells.UnionWith(newSection.GetAllPositions());
            return allCells.Count;
        }

        public int OpeningWidthMerging(RectInt newSection)
        {
            // For overlapping sctions, the merging width
            // is the wisth of the gap created by the merge,
            // computed by looking at the width and height of
            // the overlapping rectangle

            int maxWidth = 0;
            foreach (var section in sections)
            {
                if (section.Overlaps(newSection))
                {
                    var yTop = Mathf.Min(section.yMax, newSection.yMax);
                    var yBottom = Mathf.Max(section.yMin, newSection.yMin);
                    var yWidth = yTop - yBottom;

                    var xRight = Mathf.Min(section.xMax, newSection.xMax);
                    var xLeft = Mathf.Max(section.xMin, newSection.xMin);
                    var xWidth = xRight - xLeft;

                    maxWidth = Mathf.Max(maxWidth, yWidth, xWidth);
                }
            }

            Assert.IsTrue(maxWidth > 0);

            return maxWidth;
        }
    }

    public class RoomLayout
    {
        private IReadOnlyList<Room> rooms;
        private IReadOnlyDictionary<Vector2Int, Room> layout;

        public RoomLayout(IReadOnlyList<Room> rooms, IReadOnlyDictionary<Vector2Int, Room> layout)
        {
            this.rooms = rooms;
            this.layout = layout;
        }

        public IReadOnlyList<Room> AllRooms => rooms;
        public IReadOnlyDictionary<Vector2Int, Room> Layout => layout;
    }

    public class Hallway
    {
        private HashSet<Vector2Int> path;
        private IReadOnlyDictionary<Room, Vector2Int> doorMapping;

        public Hallway(HashSet<Vector2Int> path, IReadOnlyDictionary<Room, Vector2Int> doorMapping)
        {
            this.path = path;
            this.doorMapping = doorMapping;
        }

        public IEnumerable<Vector2Int> GetCells() => path;
        public IReadOnlyDictionary<Room, Vector2Int> DoorMapping => doorMapping;
        public ISet<Vector2Int> Path => path;
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

        private static bool CanMergeRooms(IEnumerable<Room> rooms, RectInt section, Parameters parameters)
        {
            var numSections = rooms.Aggregate(0, (a, r) => a + r.SectionCount) + 1;
            var numCells = rooms.Aggregate(0, (a, r) => a + r.SizeMerging(section));
            var minMergeWidth = rooms.Min(r => r.SizeMerging(section));

            return numSections <= parameters.MaximumSectionsPerRoom &&
                numCells < Mathf.FloorToInt(parameters.MaximumCellularSectorSize.x * parameters.MaximumCellularSectorSize.y * parameters.MaximumRoomSizeScale) &&
                minMergeWidth > parameters.MinimumSectorOverlapWidth;
        }

        private static bool TryInsertSection(
            RectInt section,
            IDictionary<Vector2Int, Room> lookup,
            Parameters parameters,
            out Room updatedRoom)
        {
            Room someRoom = null;

            // Get all the rooms this new section would overlap with
            HashSet<Room> overlappingRooms = new HashSet<Room>();
            foreach (var pos in section.allPositionsWithin)
            {
                if (lookup.TryGetValue(pos, out var room))
                {
                    overlappingRooms.Add(room);
                    someRoom = room;
                }
            }

            if (overlappingRooms.Count == 0)
            {
                // No overlapping rooms, just create a new one
                updatedRoom = new Room(section);
                return true;
            }
            else if (CanMergeRooms(overlappingRooms, section, parameters))
            {
                Room newRoom = new Room(section);
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

        public static RoomLayout GenerateRooms(Parameters parameters)
        {
            Vector2Int worldSize = parameters.CellularDimensions;

            Dictionary<Vector2Int, Room> roomLookup = new Dictionary<Vector2Int, Room>();
            HashSet<Room> allRooms = new HashSet<Room>();

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

                x += 2;
                y += 2;

                RectInt section = new RectInt(x, y, width, height);

                if (TryInsertSection(section, roomLookup, parameters, out var room))
                {
                    allRooms.Add(room);
                    foreach (var cell in section.allPositionsWithin)
                        roomLookup[cell] = room;

                    consecutiveFailedAttempts = 0;
                }
                else
                {
                    consecutiveFailedAttempts++;
                }
            }

            return new RoomLayout(new List<Room>(allRooms), roomLookup);
        }
    }

    private static class HallwayGenerator
    {
        private static int MAX_BINARY_PAIRED_HALLWAYS = 1;

        private static Vector2Int[] DIRECTIONS = new Vector2Int[]
        {
        Vector2Int.up,
        Vector2Int.right,
        Vector2Int.down,
        Vector2Int.left,
        };

        public static List<Hallway> GenerateHallways(RoomLayout rooms, Parameters parameters)
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
            List<Hallway> validHallways = GetConnectingHallways(maze, rooms);

            // Filter the hallways futher, removing redundant hallways and removing
            // dead ends
            List<Hallway> reducedHallways = ReduceHallways(validHallways);

            return reducedHallways;
        }

        private static List<Hallway> ReduceHallways(List<Hallway> validHallways)
        {
            List<Hallway> newHallways = new List<Hallway>();
            Dictionary<Tuple<Room, Room>, int> binaryPairs = new Dictionary<Tuple<Room, Room>, int>();

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

            bool ShouldIncludeBinaryPair(Room a, Room b)
            {
                int count = 0;

                if (binaryPairs.TryGetValue(Tuple.Create(a, b), out var count1))
                    count += count1;

                if (binaryPairs.TryGetValue(Tuple.Create(b, a), out var count2))
                    count += count2;

                // Count of 1 means we already did one. So this is strictly less-than
                return count < MAX_BINARY_PAIRED_HALLWAYS;
            }

            foreach (var hallway in validHallways)
            {
                var someDoor = hallway.DoorMapping.Values.First(_ => true);
                HashSet<Vector2Int> newPath = new HashSet<Vector2Int>();

                // Special case is only one door, there's no path to search for,
                // but we want to keep the path
                if (hallway.Path.Count == 1)
                {
                    var rooms = hallway.DoorMapping.Keys.ToArray();
                    Assert.AreEqual(rooms.Length, 2);
                    if (ShouldIncludeBinaryPair(rooms[0], rooms[1]))
                    {
                        newHallways.Add(hallway);

                        var tuple = Tuple.Create(rooms[0], rooms[1]);
                        binaryPairs.TryGetValue(tuple, out int newCount);
                        binaryPairs[tuple] = newCount + 1;
                    }
                    
                    continue;
                }

                foreach (var destDoor in hallway.DoorMapping.Values.Where(d => d != someDoor))
                {
                    newPath.UnionWith(SearchPath(someDoor, destDoor, hallway.Path));
                }

                newHallways.Add(new Hallway(newPath, hallway.DoorMapping));
            }

            return newHallways;
        }

        private static List<Hallway> GetConnectingHallways(bool[,] maze, RoomLayout rooms)
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
            List<Hallway> validHallways = new List<Hallway>();

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

                    Dictionary<Room, Vector2Int> entrances = new Dictionary<Room, Vector2Int>();
                    foreach (var (entrance, room) in doorMapping)
                        entrances[room] = entrance;

                    if (entrances.Count > 1)
                        validHallways.Add(new Hallway(path, entrances));

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

    public static WorldLayout Generate(Parameters parameters)
    {
        var rooms = RoomGenerator.GenerateRooms(parameters);
        var hallways = HallwayGenerator.GenerateHallways(rooms, parameters);
        return new WorldLayout(rooms, hallways);
    }
}