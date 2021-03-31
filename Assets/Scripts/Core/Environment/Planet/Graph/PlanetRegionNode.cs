using System;

public class PlanetRegionNode<T>
{
    private int index;
    private PlanetRegionGraph<T> graph;
    private PlanetRegionOrientation orientation;
    private T data;

    public T Data => data;
    public int Index => index;
    public PlanetRegionOrientation Orientation => orientation;

    public PlanetRegionNode(int index, PlanetRegionGraph<T> graph, PlanetRegionOrientation orientation, T data)
    {
        this.index = index;
        this.graph = graph;
        this.orientation = orientation;
        this.data = data;
    }


    public PlanetRegionNode<T> GetNeighbor(int neighbor)
    {
        return graph.GetNode(index, neighbor);
    }
}
