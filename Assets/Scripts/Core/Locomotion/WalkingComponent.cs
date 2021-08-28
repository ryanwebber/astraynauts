using UnityEngine;
using System.Collections;

[RequireComponent(typeof(ComponentBehavior))]
public class WalkingComponent : MonoBehaviour, BehaviorControlling
{
    [SerializeField]
    private WalkingActor actor;
    public WalkingActor Actor => actor;
    public ComponentBehavior Behavior { get; private set; }

    private void Awake()
    {
        Behavior = GetComponent<ComponentBehavior>()
            .Bind(actor)
            .BindOnEnable((ref Event ev) =>
            {
                ev += () => actor.EraseMomentum();
            });
    }
}
