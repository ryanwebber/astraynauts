using UnityEngine;
using System.Collections;

public class InspectorRefreshable : MonoBehaviour
{
    [HideInInspector]
    public string ButtonText = "Refresh";

    public Event OnInspectorRefresh;
}
