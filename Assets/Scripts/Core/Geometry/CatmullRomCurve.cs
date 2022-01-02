using UnityEngine;

// a single catmull-rom curve
public struct CatmullRomCurve {

    public Vector2 p0, p1, p2, p3;
    public float alpha;

    public CatmullRomCurve( Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float alpha ) {
        ( this.p0, this.p1, this.p2, this.p3 ) = ( p0, p1, p2, p3 );
        this.alpha = alpha;
    }

    // Evaluates a point at the given t-value from 0 to 1
    public Vector2 GetPoint( float t ) {
        // calculate knots
        const float k0 = 0;
        float k1 = GetKnotInterval( p0, p1 );
        float k2 = GetKnotInterval( p1, p2 ) + k1;
        float k3 = GetKnotInterval( p2, p3 ) + k2;

        // evaluate the point
        float u = Mathf.LerpUnclamped( k1, k2, t );
        Vector2 A1 = Remap( k0, k1, p0, p1, u );
        Vector2 A2 = Remap( k1, k2, p1, p2, u );
        Vector2 A3 = Remap( k2, k3, p2, p3, u );
        Vector2 B1 = Remap( k0, k2, A1, A2, u );
        Vector2 B2 = Remap( k1, k3, A2, A3, u );
        return Remap( k1, k2, B1, B2, u );
    }

    static Vector2 Remap( float a, float b, Vector2 c, Vector2 d, float u ) {
        return Vector2.LerpUnclamped( c, d, ( u - a ) / ( b - a ) );
    }

    float GetKnotInterval( Vector2 a, Vector2 b ) {
        return Mathf.Pow( Vector2.SqrMagnitude( a - b ), 0.5f * alpha );
    }
}
