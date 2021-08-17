using UnityEngine;
using System.Collections;

public class DoorController : MonoBehaviour
{
    public struct State
    {
        public bool isOpen;
    }

    public Event<State> OnDoorStateChanged;
}
