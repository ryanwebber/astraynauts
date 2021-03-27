using System;

public class PlanetRegionNode<T>
{
    private int index;
    private PlanetRegionGraph<T> graph;
    private PlanetRegionOrientation orientation;
    private T data;

    public PlanetRegionNode(int index, PlanetRegionGraph<T> graph, PlanetRegionOrientation orientation, T data)
    {
        this.index = index;
        this.graph = graph;
        this.orientation = orientation;
        this.data = data;
    }


    public PlanetRegionNode<T> GetNeighbor(int neightbor)
    {
        return graph.GetNode(index, neightbor);
    }
}
