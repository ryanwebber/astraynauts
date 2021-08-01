using System;
using UnityEngine;

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
            this.mapFn = mb => mb.GetComponent<Fixture>();
    }

    public void Perform()
    {
        var instance = UnityEngine.Object.Instantiate(prefab);
        var fixture = mapFn.Invoke(instance);
        decoratorFn?.Invoke(instance);
        fixture.RegisterInWorld(grid);
    }
}
