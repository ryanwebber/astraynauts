using System;
using UnityEngine;

public static class PlanetGeometry
{
    private static float R_NORM = (1f + Mathf.Sqrt(5)) / 2f;

    private static Vector3[] vertices = new[]
    {
        new Vector3( R_NORM,  R_NORM,  R_NORM),
        new Vector3( R_NORM,  R_NORM, -R_NORM),
        new Vector3( R_NORM, -R_NORM,  R_NORM),
        new Vector3( R_NORM, -R_NORM, -R_NORM),
        new Vector3(-R_NORM,  R_NORM,  R_NORM),
        new Vector3(-R_NORM,  R_NORM, -R_NORM),
        new Vector3(-R_NORM, -R_NORM,  R_NORM),
        new Vector3(-R_NORM, -R_NORM, -R_NORM),


    };
}
