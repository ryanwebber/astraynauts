using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Mob))]
public class StagedMobController : MonoBehaviour
{
    [System.Serializable]
    private struct Lifecycle
    {
        [SerializeField]
        public Stage spawnStage;

        [SerializeField]
        public Stage mainStage;

        [SerializeField]
        public Stage deathStage;
    }

    [SerializeField]
    private StageController controller;

    [SerializeField]
    private Lifecycle lifecycle;

    [SerializeField, ReadOnly]
    private string currentStageName;

    private void Awake()
    {
        var mob = GetComponent<Mob>();

        mob.OnMobSpawn += () =>
        {
            controller.PlaySequence(new Stage[] { lifecycle.spawnStage, lifecycle.mainStage });
        };

        mob.OnMobDefeated += () =>
        {
            controller.PlayStage(lifecycle.deathStage);
        };

        controller.OnStageChanged += () => currentStageName = controller.CurrentStage.name;
    }
}
