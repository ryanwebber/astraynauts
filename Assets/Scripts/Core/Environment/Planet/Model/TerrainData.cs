using System;
using System.Collections.Generic;

public class TerrainData
{
    private IEnumerable<ITerrainInstruction> instructions;
    public IEnumerable<ITerrainInstruction> Instructions => instructions;

    public TerrainData(IEnumerable<ITerrainInstruction> instructions)
    {
        this.instructions = instructions;
    }
}
