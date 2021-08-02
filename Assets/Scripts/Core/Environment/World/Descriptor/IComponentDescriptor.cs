using System;

public interface IComponentDescriptor<T>
{
    T Component { get; }
}
