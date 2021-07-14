using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class SettingsManager: MonoBehaviour
{
    private SettingsManager instance;
    public SettingsManager Instance => instance;

    private void Awake()
    {
        Assert.IsNull(instance);
        instance = this;
    }

    private void OnDestroy()
    {
        instance = null;
    }
}
