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
                float minSectWidth = parameters.MinimumCellularSectorSize.x;
                float maxSectWidth = Mathf.Lerp(parameters.MaximumCellularSectorSize.x, parameters.MinimumCellularSectorSize.x, t);
                float minSectHeight = parameters.MinimumCellularSectorSize.y;
                float maxSectHeight = Mathf.Lerp(parameters.MaximumCellularSectorSize.y, parameters.MinimumCellularSectorSize.y, t);

                int width = RandomUtils.RandomOddInRange(parameters.MinimumCellularSectorSize.x, Mathf.CeilToInt(maxSectWidth));
                int height = RandomUtils.RandomOddInRange(parameters.MinimumCellularSectorSize.y, Mathf.CeilToInt(maxSectHeight));

                int x = RandomUtils.RandomOddInRange(1, parameters.CellularDimensions.x - width);
                int y = RandomUtils.RandomOddInRange(1, parameters.CellularDimensions.y - height);

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

    public static class HallwayGenerator
    {
        public static bool[,] GenerateHallways(Parameters parameters)
        {
            // Total generated maze size is 4 units bigger per dimension, giving
            // us a 2-width wide border around the normal world size. We place
            // hallways on odd-squares, so this would put halls on the inner border.
            // With 1 cell of padding (again due to the odd number rule) for room
            // generation, this will let us put halls around the edge of the map.
            Vector2Int gridSize = parameters.CellularDimensions + Vector2Int.one * 4;

            bool[,] maze = new bool[gridSize.y, gridSize.x];

            IEnumerable<Vector2Int> GetNeighbors(Vector2Int pos, bool type) => new Vector2Int[]
                {
                    Vector2Int.up,
                    Vector2Int.right,
                    Vector2Int.down,
                    Vector2Int.left,
                }
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
    
    public static RoomLayout Generate(Parameters parameters)
    {
        return RoomGenerator.GenerateRooms(parameters);
    }
}
