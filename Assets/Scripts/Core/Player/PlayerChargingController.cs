using UnityEngine;
using System.Collections;

[RequireComponent(typeof(ComponentBehavior))]
public class PlayerChargingController : MonoBehaviour, BehaviorControlling
{
    [SerializeField]
    [Min(0f)]
    public float chargingStepTime;

    [SerializeField]
    public BatteryManager batteryManager;

    public Event OnChargingStep;
    public Event OnChargingBegin;
    public Event OnChargingEnd;

    public ComponentBehavior Behavior { get; private set; }

    private void Awake()
    {
        this.Behavior = GetComponent<ComponentBehavior>()
            .BindCoroutines(this)
            .BindOnEnable((ref Event ev) =>
            {
                ev += () => StartCoroutine(Charge());
                ev += () => OnChargingBegin?.Invoke();
            })
            .BindOnDisable((ref Event ev) =>
            {
                ev += () => OnChargingEnd?.Invoke();
            });

        OnChargingStep += () => batteryManager.AddBatteryValue(1);
    }

    private IEnumerator Charge()
    {
        while (true)
        {
            yield return new WaitForSeconds(chargingStepTime);
            OnChargingStep?.Invoke();
        }
    }
}
