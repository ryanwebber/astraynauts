using UnityEngine;
using System.Collections.Generic;

public abstract class BaseInfluencer : MonoBehaviour
{
    public abstract IEnumerable<Vector2> GetInfluences();
}
