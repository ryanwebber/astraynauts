using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using static WorldGenerator;
using System.Linq;

public class Temporary : MonoBehaviour
{
    [SerializeField]
    private WorldGenerator.Parameters generationParameters;

    private RoomLayout rooms;
    private List<Hallway> hallways;

    private void Awake()
    {
        if (TryGetComponent<InspectorRefreshable>(out var refreshable))
            refreshable.OnInspectorRefresh += GenerateWorld;
    }

    private void Start()
    {
        GenerateWorld();
    }

    private void GenerateWorld()
    {
        rooms = Profile.Debug("Generate World Layout", () => WorldGenerator.Generate(generationParameters));
        hallways = Profile.Debug("Generate Hallways", () => WorldGenerator.HallwayGenerator.GenerateHallways(rooms, generationParameters));
    }

    private void OnDrawGizmos()
    {
        if (rooms == null)
            return;

        Vector2Int gridSize = generationParameters.CellularDimensions + Vector2Int.one * 4;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(0.5f * new Vector3(gridSize.x, gridSize.y, 0f), new Vector3(gridSize.x, gridSize.y, 1f));

        foreach (var room in rooms.AllRooms)
        {
            var hue = Mathf.Abs(room.GetHashCode());
            var color = Color.HSVToRGB(Mathf.Clamp01((hue % 255) / 255f), 1, 1);

            Gizmos.color = color;
            foreach (var section in room.GetSections())
            {
                Gizmos.DrawCube(section.center, new Vector3(section.size.x, section.size.y, 1f));
            }
        }

        if (hallways == null)
            return;

        Gizmos.color = new Color(1f, 1f, 1f, 0.5f);

        HashSet<Vector2Int> doors = new HashSet<Vector2Int>();
        foreach (var hallway in hallways)
        {
            doors.Clear();
            foreach (var door in hallway.DoorMapping.Values)
                doors.Add(door);

            foreach (var cell in hallway.GetCells())
            {
                Gizmos.color = doors.Contains(cell) ? Color.yellow : Color.white;
                Gizmos.DrawCube(new Vector3(cell.x + 0.5f, cell.y + 0.5f, 1f), Vector3.one);
            }
        }
    }
}
