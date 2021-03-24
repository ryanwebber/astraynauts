using System;

public class PlanetRegionTilePainter: IPlanetRegionTilePainter
{
    private ITerrainLayerManager layerManager;

    public PlanetRegionTilePainter(ITerrainLayerManager layerManager)
    {
        this.layerManager = layerManager;
    }

    public IMutableTerrainLayer GetLayer()
    {
        return layerManager.GetLayer();
    }
}
