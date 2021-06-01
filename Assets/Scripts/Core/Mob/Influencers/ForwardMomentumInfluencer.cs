using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Heading2D))]
public class ForwardMomentumInfluencer : MonoBehaviour
{
    [SerializeField]
    private float weight = 1f;

    private Heading2D heading;

    private void Awake()
    {
        heading = GetComponent<Heading2D>();
    }

    public IEnumerable<Vector2> GetInfluences()
    {
        yield return heading.CurrentHeading * weight;
    }
}
