using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PlayerState))]
public class PlayerAnimationController : MonoBehaviour
{
    [SerializeField]
    private PlayerState playerState;

    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();

        playerState.OnBatteryChargeStateEnter += () =>
        {
            animator.SetBool("IsWalking", false);
            animator.SetBool("IsDashing", false);
        };

        playerState.MovementController.OnDashStateEnter += () =>
        {
            animator.SetBool("IsDashing", true);
        };
    }

    private void Update()
    {
        var playerVelocity = playerState.MovementController.WalkingActor.CurrentVelocity;

        if (playerVelocity.sqrMagnitude > 0.01f)
        {
            animator.SetInteger("MovementDirection", DirectionToClockwiseInteger(playerVelocity));
            animator.SetBool("IsWalking", true);
        }
        else
        {
            animator.SetBool("IsWalking", false);
        }

        animator.SetInteger("FacingDirection", DirectionToClockwiseInteger(playerState.ShootingController.AimInputValue));
    }

    // TODO: This doesn't work right
    private int DirectionToClockwiseInteger(Vector2 dir)
    {
        var offset = 180f + (360f / 16f);
        var theta = -1 * Vector2.SignedAngle(Vector2.down, dir) + offset;
        return Mathf.FloorToInt(Mathf.InverseLerp(offset - 180f, 180f + offset, theta) * 8f) % 8;
    }
}
