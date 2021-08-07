using UnityEngine;
using System.Collections;

public class HealthManager : MonoBehaviour
{
    [SerializeField]
    private HealthProperties properties;
    public HealthProperties Properties => properties;

    [SerializeField]
    private int healthValue;
    public int HealthValue => healthValue;

    public bool CanTakeDamage = true;

    public Event<PropertyChange<int>, HealthManager> OnHealthValueChanged;
    public Event<PropertyChange<HealthProperties>, HealthManager> OnHealthPropertiesChanged;

    private void Awake()
    {
        OnHealthPropertiesChanged += (change, _) =>
        {
            Debug.Log($"Health changed from {change.oldValue} to {change.newValue}", this);
        };
    }

    public PropertyChange<int> SetHealthValue(int value)
    {
        value = Mathf.Clamp(value, 0, properties.MaxHealth);
        if (value != healthValue && CanTakeDamage)
        {
            var prevValue = healthValue;
            healthValue = value;

            var delta = new PropertyChange<int>(prevValue, value);
            OnHealthValueChanged?.Invoke(delta, this);

            return delta;
        }

        return new PropertyChange<int>(healthValue, healthValue);
    }

    public PropertyChange<int> AddHealth(int delta)
    {
        return SetHealthValue(healthValue + delta);
    }

    public void UpdateProperties(PropertiesUpdating<HealthProperties> updater)
    {
        var oldProps = properties;
        updater?.Invoke(ref properties);

        if (properties != oldProps)
        {
            OnHealthPropertiesChanged?.Invoke(new PropertyChange<HealthProperties>(oldProps, properties), this);
        }
    }

    private void OnValidate()
    {
        healthValue = Mathf.Clamp(healthValue, 0, properties.MaxHealth);
    }
}
