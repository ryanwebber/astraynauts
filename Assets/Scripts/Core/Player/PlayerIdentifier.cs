using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIdentifier
{
    public int PlayerIndex { get; }
    public string Scheme { get; }

    public string Key => $"{PlayerIndex};${Scheme}";

    public PlayerIdentifier(int playerIndex, string scheme)
    {
        PlayerIndex = playerIndex;
        Scheme = scheme;
    }

    public static bool TryParse(string key, out PlayerIdentifier identifier)
    {
        var segments = key.Split(';');
        if (segments.Length >= 2 && int.TryParse(segments[0], out var playerIndex))
        {
            identifier = new PlayerIdentifier(playerIndex, segments[1]);
            return true;
        }

        identifier = default;
        return false;
    }

    public static PlayerIdentifier Default
    {
        get
        {
            if (Application.isConsolePlatform)
                return new PlayerIdentifier(0, InputScemeUtils.GamepadScheme);
            else
                return new PlayerIdentifier(0, InputScemeUtils.KeyboardAndMouseScheme);
        }
    }
}
