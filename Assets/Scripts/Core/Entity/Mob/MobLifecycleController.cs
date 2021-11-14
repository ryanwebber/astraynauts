using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Mob))]
public class MobLifecycleController : MonoBehaviour
{
    // The bounds of the main state
    public Event OnBeginMobControl;
    public Event OnEndMobControl;
}
