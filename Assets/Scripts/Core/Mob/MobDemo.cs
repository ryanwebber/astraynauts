﻿using UnityEngine;
using System.Collections;

// TODO: Delete this
[RequireComponent(typeof(Mob))]
[RequireComponent(typeof(RectGizmo))]
public class MobDemo : MonoBehaviour
{
    private void Awake()
    {
        var mob = GetComponent<Mob>();
        mob.OnDidSpawnIntoWorld += () =>
        {
            GetComponent<RectGizmo>().color = Color.cyan;
            StartCoroutine(Coroutines.After(5f, () =>
            {
                mob.OnMobDefeated?.Invoke();
                Destroy(gameObject);
            }));
        };
    }
}