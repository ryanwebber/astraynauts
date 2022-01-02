using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(RopeSystem))]
public class Cable : MonoBehaviour
{
    [SerializeField] private float distanceBetweenPoints = 0.2f;
    [SerializeField] private int numPoints = 24;
    
    [Header("Temporary")]
    
    [SerializeField]
    private GameState gameState;

    private RopeSystem ropeSystem;
    private Player follow;

    private void Awake()
    {
        ropeSystem = GetComponent<RopeSystem>();
        gameState.Services.PlayerManager.OnPlayerSpawnComplete += () =>
        {
            var player = Enumerable.First(gameState.Services.PlayerManager.GetAlivePlayers());
            var origin = (Vector2)player.transform.position;
            var points = new List<Vector2>();

            for (int i = 0; i < numPoints; i++)
            {
                var position = origin + Random.insideUnitCircle.normalized * Random.Range(0.5f, 1f);
                points.Add(position);
            }

            ropeSystem.SetPoints(points, distanceBetweenPoints);
            ropeSystem.SetPointLocked(0, true);
            follow = player;
        };
    }

    private void Update()
    {
        if (follow == null || ropeSystem == null || ropeSystem.PointCount <= 0)
            return;
        
        ropeSystem.SetPoint(0, follow.WorldPosition);
        ropeSystem.Solve();
    }
}
