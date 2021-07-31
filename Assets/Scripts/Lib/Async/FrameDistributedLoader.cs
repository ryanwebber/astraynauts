using System;
using System.Collections.Generic;
using UnityEngine;

public class FrameDistributedLoader
{
    public struct LoadState
    {
        public int frameCount;
        public int operationCount;
        public float duration;
    }

    public static float MAXMIMUM_LOAD_TIME_PER_FRAME_MS = 30f;

    private IEnumerable<IOperation>[] operations;

    public FrameDistributedLoader(params IEnumerable<IOperation>[] operations)
    {
        this.operations = operations;
    }

    public IEnumerable<LoadState> SparseLoad()
    {
        float startTime = Time.time;
        int frameCount = 0;
        int opCount = 0;
        foreach (var set in operations)
        {
            foreach (var op in set)
            {
                if (FrameTimer.Instance.FrameDuration > MAXMIMUM_LOAD_TIME_PER_FRAME_MS)
                {
                    frameCount++;

                    yield return new LoadState
                    {
                        frameCount = frameCount,
                        operationCount = opCount,
                        duration = Time.time - startTime,
                    };
                }

                op.Perform();
                opCount++;
            }
        }
    }
}
