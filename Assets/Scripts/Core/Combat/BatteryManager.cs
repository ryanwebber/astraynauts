using UnityEngine;
using System.Collections;

public class BatteryManager : MonoBehaviour, IPropertiesMutable<BatteryProperties>
{
    [SerializeField]
    private BatteryProperties properties;
    public BatteryProperties Properties => properties;

    [SerializeField]
    private int batteryValue;
    public int BatteryValue => batteryValue;

    public Event<PropertyChange<int>, BatteryManager> OnBatteryLevelChanged;
    public Event<PropertyChange<BatteryProperties>, BatteryManager> OnBatteryPropertiesChanged;

    private void Awake()
    {
        OnBatteryLevelChanged += (change, _) =>
        {
            Debug.Log($"Battery level changed from {change.oldValue} to {change.newValue}", this);
        };
    }

    public void SetBatteryValue(int value)
    {
        value = Mathf.Clamp(value, 0, properties.MaxCharge);
        if (value != batteryValue)
        {
            var prevValue = batteryValue;
            batteryValue = value;
            OnBatteryLevelChanged?.Invoke(new PropertyChange<int>(prevValue, value), this);
        }
    }

    public void AddBatteryValue(int delta)
    {
        SetBatteryValue(batteryValue + delta);
    }

    public void UpdateProperties(PropertiesUpdating<BatteryProperties> updater)
    {
        var oldProps = properties;
        updater?.Invoke(ref properties);

        if (properties != oldProps)
        {
            OnBatteryPropertiesChanged?.Invoke(new PropertyChange<BatteryProperties>(oldProps, properties), this);
        }
    }

    private void OnValidate()
    {
        batteryValue = Mathf.Clamp(batteryValue, 0, properties.MaxCharge);
    }
}
