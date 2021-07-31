using System;

public struct PropertyChange<T>
{
    public T oldValue;
    public T newValue;

    public PropertyChange(T oldValue, T newValue)
    {
        this.oldValue = oldValue;
        this.newValue = newValue;
    }
}
