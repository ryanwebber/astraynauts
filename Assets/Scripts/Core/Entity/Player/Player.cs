using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PlayerInputFeedback))]
[RequireComponent(typeof(PlayerInputBinder))]
[RequireComponent(typeof(GridLockedBody))]
public class Player : MonoBehaviour
{
    public Event<GameState> OnPlayerWillSpawn;

    public PlayerIdentifier Identifier { get; private set; }
    public PlayerInputBinder InputBinder { get; private set; }
    public PlayerInputFeedback InputFeedback { get; private set; }

    public Vector2 WorldPosition => transform.position;

    // Used to re-align the player to a grid position
    private GridLockedBody gridBody;

    private void Awake()
    {
        Identifier = PlayerIdentifier.Default;

        InputFeedback = GetComponent<PlayerInputFeedback>();
        InputBinder = GetComponent<PlayerInputBinder>();

        gridBody = GetComponent<GridLockedBody>();

        InputBinder.OnAttachToInput += input => Identifier = input.InputIdentifier;
    }

    public Vector2 GetSnapPointToGrid(World world)
    {
        return GetSnapPointToGrid(world, gridBody.WorldPosition);
    }

    public Vector2 GetSnapPointToGrid(World world, Vector2 position)
    {
        var currentUnit = world.WorldPositionToUnit(position);
        var naiveSnappedPosition = world.UnitBounds(currentUnit).center;
        var correctedSnappedPosition = naiveSnappedPosition - gridBody.Offset;
        return correctedSnappedPosition;
    }
}
