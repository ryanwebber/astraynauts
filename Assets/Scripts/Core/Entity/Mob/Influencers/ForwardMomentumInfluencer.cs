using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ForwardMomentumInfluencer : BaseInfluencer
{
    [SerializeField]
    private Heading2D heading;

    public override IEnumerable<Vector2> GetInfluences()
    {
        yield return heading.CurrentHeading;
    }
}
