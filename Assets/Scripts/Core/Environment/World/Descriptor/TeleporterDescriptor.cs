using System;
public class TeleporterDescriptor: WorldGrid.Descriptor, IComponentDescriptor<Teleporter>
{
    public readonly Teleporter Teleporter;

    public TeleporterDescriptor(Teleporter teleporter)
    {
        Teleporter = teleporter;
    }

    Teleporter IComponentDescriptor<Teleporter>.Component => Teleporter;
}
