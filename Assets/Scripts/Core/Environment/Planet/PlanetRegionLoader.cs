using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PlanetRegion))]
public class PlanetRegionLoader : MonoBehaviour
{
    [SerializeField]
    private PlanetRegionEdge edgePrefab;

    private PlanetRegion region;

    private void Awake()
    {
        region = GetComponent<PlanetRegion>();
    }

    private void Start()
    {
        var inlayedPentagon = Pentagon.WithCircumradius(region.Bounds.Circumradius - 2f);
        foreach (var edge in region.Bounds.GetEdges())
        {
            var edgeInstance = Instantiate(edgePrefab, transform);
            edgeInstance.SetEdge(edge);
        }
    }
}
