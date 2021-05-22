using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public static class Profile
{
    public static void Debug(string description, System.Action action)
    {
        Stopwatch st = new Stopwatch();
        st.Start();
        action?.Invoke();
        st.Stop();
        UnityEngine.Debug.Log($"[{st.ElapsedMilliseconds}ms] {description}");
    }

    public static T Debug<T>(string description, System.Func<T> action)
    {
        Stopwatch st = new Stopwatch();
        st.Start();
        var value = action.Invoke();
        st.Stop();
        UnityEngine.Debug.Log($"[{st.ElapsedMilliseconds}ms] {description}");
        return value;
    }
}
