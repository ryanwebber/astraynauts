using System;

public interface ITerrainInstruction
{
    void Paint(PlanetRegionShape shape, IPlanetRegionTilePainter painter);
}
