using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Velocity2D))]
public class ProjectileRotator : MonoBehaviour
{
    [SerializeField]
    private List<Transform> spriteTransforms;

    [SerializeField]
    [Range(-360f, 360f)]
    private float angleOffset = 180f;

    private Velocity2D velocityContainer;

    private void Awake()
    {
        velocityContainer = GetComponent<Velocity2D>();
    }

    private void Update()
    {
        if (spriteTransforms != null && velocityContainer.CurrentVelocity != Vector2.zero)
        {
            float zRot = Mathf.Atan2(velocityContainer.CurrentVelocity.x, -velocityContainer.CurrentVelocity.y) * Mathf.Rad2Deg;
            foreach (var st in spriteTransforms)
                st.rotation = Quaternion.Euler(0f, 0f, zRot + angleOffset);
        }
    }
}
