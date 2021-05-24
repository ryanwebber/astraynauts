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
    private bool[,] maze;

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
        maze = WorldGenerator.HallwayGenerator.GenerateHallways(generationParameters);
    }    

    private void OnDrawGizmos()
    {
        if (maze == null)
            return;

        Gizmos.color = Color.white;
        for (int y = 0; y < maze.GetLength(0); y++)
        {
            for (int x = 0; x < maze.GetLength(1); x++)
            {
                if (maze[y,x])
                {
                    // This -2 here (1.5 due to drawing at the center) is the extra border
                    // the hallways take to be able to be placed around the outside of rooms
                    Gizmos.DrawCube(new Vector3(x - 1.5f, y - 1.5f, 1f), Vector3.one);
                }
            }
        }

        if (rooms == null)
            return;

        Vector2Int gridSize = generationParameters.CellularDimensions;

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
    }
}
