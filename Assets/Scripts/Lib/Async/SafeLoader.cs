using System;
using System.Collections.Generic;

public class SafeLoader
{
    public static float MAXMIMUM_LOAD_TIME_PER_FRAME_MS = 30f;

    private IEnumerable<IOperation>[] operations;

    public SafeLoader(params IEnumerable<IOperation>[] operations)
    {
        this.operations = operations;
    }

    public IEnumerable<T> SparseLoad<T>(T step)
    {
        foreach (var set in operations)
        {
            foreach (var op in set)
            {
                op.Perform();
                if (FrameTimer.Instance.FrameDuration > MAXMIMUM_LOAD_TIME_PER_FRAME_MS)
                    yield return step;
            }
        }
    }
}
