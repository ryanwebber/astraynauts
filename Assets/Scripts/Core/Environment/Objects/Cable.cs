using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(RopeSystem))]
public class Cable : MonoBehaviour
{
    [Header("Temporary")]
    
    [SerializeField]
    private GameState gameState;

    private RopeSystem ropeSystem;
    private Transform follow;

    private void Awake()
    {
        ropeSystem = GetComponent<RopeSystem>();
        gameState.Services.PlayerManager.OnPlayerSpawnComplete += () =>
        {
            var player = Enumerable.First(gameState.Services.PlayerManager.GetAlivePlayers());
            var origin = (Vector2)player.transform.position;
            var points = new List<Vector2>();

            for (int i = 0; i < 16; i++)
            {
                var position = origin + Random.insideUnitCircle.normalized * Random.Range(0.5f, 1f);
                points.Add(position);
            }

            ropeSystem.SetPoints(points, 0.4f);
            ropeSystem.SetPointLocked(0, true);
            follow = player.transform;
        };
    }

    private void Update()
    {
        if (follow == null || ropeSystem == null || ropeSystem.PointCount <= 0)
            return;
        
        ropeSystem.SetPoint(0, follow.position);
        ropeSystem.Solve();
    }
}
