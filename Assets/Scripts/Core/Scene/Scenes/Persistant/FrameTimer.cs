using UnityEngine;
using System.Collections;
using System.Diagnostics;

public class FrameTimer : MonoBehaviour
{
    private static FrameTimer _instance = null;
    public static FrameTimer Instance
    {
        get
        {
            if (_instance != null)
                return _instance;

            UnityEngine.Debug.LogWarning("FrameTimer is null.");

            return null;
        }

        private set => _instance = value;
    }

    private Stopwatch stopwatch;
    public long FrameDuration
    {
        get
        {
            if (this.stopwatch == null)
                return 0;
            else
                return this.stopwatch.ElapsedMilliseconds;
        }
    }

    void Awake()
    {
        if (_instance == null)
            _instance = this;

        stopwatch = new Stopwatch();
    }

    void OnDestroy()
    {
        _instance = null;
    }

    void Update()
    {
        stopwatch.Reset();
        stopwatch.Start();
    }
}