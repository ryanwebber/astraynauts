using System;
using System.Collections;

public class TweenStep
{
    private IEnumerable enumerable;
    public IEnumerable Stacked => enumerable;

    public TweenStep()
    {
        this.enumerable = Empty();
    }

    private TweenStep(IEnumerable enumerable)
    {
        this.enumerable = enumerable;
    }

    public TweenStep Then(params IEnumerable[] other)
    {
        return new TweenStep(Sequence(enumerable, other));
    }

    private static IEnumerable Sequence(params IEnumerable[] enumerables)
    {
        foreach (var enumerable in enumerables)
            foreach (var instruction in enumerable)
                yield return instruction;
    }

    private static IEnumerable Empty()
    {
        yield break;
    }
}
