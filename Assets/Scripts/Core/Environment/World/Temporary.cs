using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Temporary : MonoBehaviour
{
    private ICollection<WorldGenerator.Room> rooms;

    private void Start()
    {
        rooms = WorldGenerator.Generate(160, 160);
        Debug.Log($"Got {rooms.Count} rooms");
    }

    private void OnDrawGizmos()
    {
        if (rooms == null)
            return;

        foreach (var room in rooms)
        {
            var hue = room.GetHashCode();
            var color = Color.HSVToRGB(Mathf.Clamp01((hue % 255) / 255f), 1, 1);

            Gizmos.color = color;
            foreach (var section in room.GetSections())
            {
                Gizmos.DrawCube(section.center, new Vector3(section.size.x, section.size.y, 1f));
            }
        }
    }
}
