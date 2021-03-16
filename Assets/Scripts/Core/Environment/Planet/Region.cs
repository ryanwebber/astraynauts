using UnityEngine;
using System.Collections;

namespace Region
{
    public class Region : MonoBehaviour
    {
        public Pentagon shape;

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            foreach (var edge in Collections.Pair(shape.GetPoints(), true))
            {
                Gizmos.DrawLine(edge.Item1, edge.Item2);
            }
        }
    }
}
