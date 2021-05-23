using System;
using System.Collections.Generic;
using Extensions;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

public static class WorldGenerator
{
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
            Assert.IsTrue(sections.Exists(rect.Overlaps));
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
        private static RangeInt VALID_MAX_SECTIONS_PER_ROOM = new RangeInt(1, 8);
        private static RangeInt VALID_CONSECUTIVE_FAILED_ROOM_INSERTION_ATTEMPTS = new RangeInt(1, 64);

        private static Vector2Int MAX_ROOM_SECT_SIZE = new Vector2Int(15, 15);
        private static Vector2Int MIN_ROOM_SECT_SIZE = new Vector2Int(7, 7);

        private static int MIN_OVERLAP_WIDTH = 4;

        private static UnitScalar CONFIG_ROOM_DENSITY = 0.1f;
        private static UnitScalar CONFIG_ROOM_REGULARITY = 0.1f;
        private static float CONFIG_MAX_ROOM_SIZE_MULTIPLE = 1.5f;

        private static AnimationCurve ROOM_SIZE_DECAY = AnimationCurve.Constant(0f, 1f, 0f);

        private static bool TryInsertSection(RectInt section, IReadOnlyDictionary<Vector2Int, Room> lookup, out Room updatedRoom)
        {
            Room someRoom = null;

            int maxCellsPerRoom = Mathf.FloorToInt(MAX_ROOM_SECT_SIZE.x * MAX_ROOM_SECT_SIZE.y * CONFIG_MAX_ROOM_SIZE_MULTIPLE);

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
            else if (overlappingRooms.Count == 1 &&
                someRoom.SectionCount < VALID_MAX_SECTIONS_PER_ROOM.Resolve(CONFIG_ROOM_REGULARITY.Inverse) &&
                someRoom.SizeMerging(section) < maxCellsPerRoom &&
                someRoom.OpeningWidthMerging(section) > MIN_OVERLAP_WIDTH)
            {
                someRoom.AddSection(section);
                updatedRoom = someRoom;
                return true;
            }
            else
            {
                updatedRoom = default;
                return false;
            }
        }

        public static RoomLayout GenerateRooms(int frameWidth, int frameHeight)
        {
            Assert.IsTrue(frameWidth > MAX_ROOM_SECT_SIZE.x + 2);
            Assert.IsTrue(frameHeight > MAX_ROOM_SECT_SIZE.y + 2);
            Assert.IsTrue(CONFIG_MAX_ROOM_SIZE_MULTIPLE >= 1f);

            Dictionary<Vector2Int, Room> roomLookup = new Dictionary<Vector2Int, Room>();
            HashSet<Room> allRooms = new HashSet<Room>();

            int consecutiveFailedAttempts = 0;
            int maxConsecutiveFailedAttempts = VALID_CONSECUTIVE_FAILED_ROOM_INSERTION_ATTEMPTS.Resolve(CONFIG_ROOM_DENSITY);

            while (consecutiveFailedAttempts < maxConsecutiveFailedAttempts)
            {
                float t = ROOM_SIZE_DECAY.Evaluate(consecutiveFailedAttempts / (float)maxConsecutiveFailedAttempts);
                float maxSectWidth = Mathf.Lerp(MAX_ROOM_SECT_SIZE.x, MIN_ROOM_SECT_SIZE.x, t);
                float maxSectHeight = Mathf.Lerp(MAX_ROOM_SECT_SIZE.y, MIN_ROOM_SECT_SIZE.y, t);

                int width = RandomUtils.RandomOddInRange(MIN_ROOM_SECT_SIZE.x, Mathf.CeilToInt(maxSectWidth));
                int height = RandomUtils.RandomOddInRange(MIN_ROOM_SECT_SIZE.y, Mathf.CeilToInt(maxSectHeight));

                int x = RandomUtils.RandomOddInRange(1, frameWidth - width);
                int y = RandomUtils.RandomOddInRange(1, frameHeight - height);

                RectInt section = new RectInt(x, y, width, height);

                if (TryInsertSection(section, roomLookup, out var room))
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
        public static void GenerateHallways(RoomLayout roomLayout)
        {

        }
    }
    
    public static IReadOnlyList<Room> Generate(int width, int height)
    {
        return RoomGenerator.GenerateRooms(width, height).AllRooms;
    }
}
