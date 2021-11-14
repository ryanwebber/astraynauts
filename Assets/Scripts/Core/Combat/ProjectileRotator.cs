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

    private void Start()
    {
        ForceUpdate();
    }

    private void Update()
    {
        ForceUpdate();
    }

    private void UpdateRotation(Vector2 vel)
    {
        if (spriteTransforms != null && vel != Vector2.zero)
        {
            float zRot = Mathf.Atan2(vel.x, -vel.y) * Mathf.Rad2Deg;
            foreach (var st in spriteTransforms)
                st.rotation = Quaternion.Euler(0f, 0f, zRot + angleOffset);
        }
    }

    public void ForceUpdate()
        => UpdateRotation(velocityContainer.CurrentVelocity);
}
