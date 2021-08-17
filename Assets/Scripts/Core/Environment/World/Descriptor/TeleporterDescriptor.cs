using System;
public class TeleporterDescriptor: WorldGrid.Descriptor
{
    public readonly Teleporter Teleporter;

    public TeleporterDescriptor(Teleporter teleporter)
    {
        Teleporter = teleporter;
    }
}
