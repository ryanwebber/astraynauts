using UnityEngine;
using System.Collections;

public class HealthManager : MonoBehaviour
{
    public enum Damagability
    {
        VULNERABLE, TRANSPARENT, SHIELDED
    }

    [SerializeField]
    private HealthProperties properties;
    public HealthProperties Properties => properties;

    [SerializeField]
    private int healthValue;
    public int HealthValue => healthValue;

    public Damagability State = Damagability.VULNERABLE;
    public bool CanTakeDamage => State == Damagability.VULNERABLE;

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
        if (value != healthValue)
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
        return SetHealthValue(healthValue + Mathf.Abs(delta));
    }

    public DamageResult DealDamage(int damage)
    {
        switch (State)
        {
            case Damagability.VULNERABLE:
                var delta = SetHealthValue(healthValue - Mathf.Abs(damage));
                return new DamageResult
                {
                    totalDamageDealt = delta.oldValue - delta.newValue,
                };
            case Damagability.TRANSPARENT:
                return new DamageResult
                {
                    targetWasTransparent = true
                };
            case Damagability.SHIELDED:
                return new DamageResult
                {
                    targetWasShielded = true
                };
        }

        throw new System.Exception("Unknown health state");
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
