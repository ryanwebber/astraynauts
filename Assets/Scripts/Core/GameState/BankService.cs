using UnityEngine;
using System.Collections;

public class BankService : MonoBehaviour
{
    public Event<PropertyChange<int>> OnValueChanged;

    [SerializeField]
    [ReadOnly]
    private int value;
    public int Value => value;

    private void Awake() => ResetValue();
    private void OnDrawGizmos() => ResetValue();
    private void ResetValue() => value = 0;

    public void AddValue(int amount)
    {
        var oldValue = value;
        value = Mathf.Max(0, oldValue + amount);
        OnValueChanged?.Invoke(new PropertyChange<int>
        {
            oldValue = oldValue,
            newValue = value
        });
    }
}
