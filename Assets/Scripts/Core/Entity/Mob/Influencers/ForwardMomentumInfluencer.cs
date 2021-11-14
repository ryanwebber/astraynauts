using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ForwardMomentumInfluencer : MonoBehaviour
{
    [SerializeField]
    private Heading2D heading;

    [SerializeField]
    private float weight = 1f;

    public IEnumerable<Vector2> GetInfluences()
    {
        yield return heading.CurrentHeading * weight;
    }
}
