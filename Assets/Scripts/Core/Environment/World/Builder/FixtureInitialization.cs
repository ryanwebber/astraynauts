using System;
using UnityEngine;
using UnityEngine.Assertions;

public class FixtureInitialization<T> : IOperation where T: MonoBehaviour
{
    private T prefab;
    private WorldGrid grid;
    private Action<T> decoratorFn;
    private Func<T, Fixture> mapFn;

    public FixtureInitialization(T prefab, WorldGrid grid, Action<T> decoratorFn = null, Func<T, Fixture> mapFn = null)
    {
        this.prefab = prefab;
        this.grid = grid;
        this.decoratorFn = decoratorFn;
        this.mapFn = mapFn;
        if (this.mapFn == null)
            this.mapFn = mb =>
            {
                if (mb is Fixture self)
                    return self;
                else
                    return mb.GetComponent<Fixture>();
            };
    }

    public void Perform()
    {
        var instance = UnityEngine.Object.Instantiate(prefab);
        var fixture = mapFn.Invoke(instance);

        Assert.IsNotNull(fixture, "Unable to extract fixture from provided prefab");

        decoratorFn?.Invoke(instance);
        fixture.RegisterInWorld(grid);
    }
}
