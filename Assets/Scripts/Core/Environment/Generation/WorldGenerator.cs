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

        public int SizeIfIncluding(RectInt newSection)
        {
            var allCells = GetUniqueCells();
            allCells.UnionWith(newSection.GetAllPositions());
            return allCells.Count;
        }
    }

    private static class RoomGenerator
    {
        private static Vector2Int MAX_ROOM_SECT_SIZE = new Vector2Int(41, 41);
        private static Vector2Int MIN_ROOM_SECT_SIZE = new Vector2Int(11, 11);
        private static int MAX_FAILED_ROOM_GENERATION_ATTEMPTS = 20;
        private static int MAX_SECTIONS_PER_ROOM = 3;
        private static int MAX_CELLS_PER_ROOM = 41 * 41 * 2;

        private static int RandomOddInRange(int min, int max)
        {
            return Random.Range(min / 2, max / 2) * 2 + 1;
        }

        private static bool TryInsertSection(RectInt section, IReadOnlyDictionary<Vector2Int, Room> lookup, out Room updatedRoom)
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
            else if (overlappingRooms.Count == 1 &&
                someRoom.SectionCount < MAX_SECTIONS_PER_ROOM &&
                someRoom.SizeIfIncluding(section) < MAX_CELLS_PER_ROOM)
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

        public static ICollection<Room> GenerateRooms(int frameWidth, int frameHeight)
        {
            Assert.IsTrue(frameWidth > MAX_ROOM_SECT_SIZE.x + 2);
            Assert.IsTrue(frameHeight > MAX_ROOM_SECT_SIZE.y + 2);

            Dictionary<Vector2Int, Room> roomLookup = new Dictionary<Vector2Int, Room>();

            int consecutiveFailedAttempts = 0;

            while (consecutiveFailedAttempts < MAX_FAILED_ROOM_GENERATION_ATTEMPTS)
            {
                int width = RandomOddInRange(MIN_ROOM_SECT_SIZE.x, MAX_ROOM_SECT_SIZE.x);
                int height = RandomOddInRange(MIN_ROOM_SECT_SIZE.y, MAX_ROOM_SECT_SIZE.y);

                int x = RandomOddInRange(1, frameWidth - width);
                int y = RandomOddInRange(1, frameHeight - height);

                RectInt section = new RectInt(x, y, width, height);

                if (TryInsertSection(section, roomLookup, out var room))
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

            return new HashSet<Room>(roomLookup.Values);
        }
    }
    
    public static ICollection<Room> Generate(int width, int height)
    {
        return RoomGenerator.GenerateRooms(width, height);
    }
}
