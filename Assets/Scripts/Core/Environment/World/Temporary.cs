using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Temporary : MonoBehaviour
{
    [SerializeField]
    private Vector2Int gridSize;

    private IEnumerable<WorldGenerator.Room> rooms;

    private void Start()
    {
        rooms = Profile.Debug("Generate World Layout", () => WorldGenerator.Generate(gridSize.x, gridSize.y));
    }

    private void OnDrawGizmos()
    {
        if (rooms == null)
            return;

        foreach (var room in rooms)
        {
            var hue = Mathf.Abs(room.GetHashCode());
            var color = Color.HSVToRGB(Mathf.Clamp01((hue % 255) / 255f), 1, 1);

            Gizmos.color = color;
            foreach (var section in room.GetSections())
            {
                Gizmos.DrawCube(section.center, new Vector3(section.size.x, section.size.y, 1f));
            }
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(0.5f * new Vector3(gridSize.x, gridSize.y, 0f), new Vector3(gridSize.x, gridSize.y, 1f));
    }
}
