using System;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "TileDistribution", menuName = "Custom/Common/Tile Distribution")]
public class TileDistribution: ScriptableObject
{
    [System.Serializable]
    public class TileEntry : IWeightedElement<TileBase>
    {
        [SerializeField]
        private TileBase tile;

        [SerializeField]
        [Min(0f)]
        private int weight;

        public int Weight => weight;
        public TileBase Value => tile;
    }

    [SerializeField]
    private TileEntry[] tiles;

    public IRandomAccessCollection<TileBase> AsCollection() => new RandomAccessCollection<TileBase>(tiles);
}
