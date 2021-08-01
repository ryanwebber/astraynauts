using System;
public class AirlockDescriptor: WorldGrid.Descriptor
{
    public readonly Airlock Airlock;

    public AirlockDescriptor(Airlock airlock)
    {
        Airlock = airlock;
    }
}
