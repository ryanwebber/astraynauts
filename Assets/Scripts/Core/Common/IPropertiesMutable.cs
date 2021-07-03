using System;

public delegate void PropertiesUpdating<TProperties>(ref TProperties properties);

public interface IPropertiesMutable<TProperties>
{
    void UpdateProperties(PropertiesUpdating<TProperties> updater);
}
