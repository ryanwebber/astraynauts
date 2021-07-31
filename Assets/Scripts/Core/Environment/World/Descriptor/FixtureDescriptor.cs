using System;
using UnityEngine;

public class FixtureDescriptor: WorldGrid.Descriptor
{
    public Fixture gameObject { get; private set; }

    public FixtureDescriptor(Fixture gameObject)
    {
        this.gameObject = gameObject;
    }
}
