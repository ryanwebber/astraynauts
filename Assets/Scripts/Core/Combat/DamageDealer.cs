using UnityEngine;
using System.Collections;

public class DamageDealer : MonoBehaviour
{
    [SerializeField]
    private LayerMask mask;

    [SerializeField]
    [Min(0)]
    private int baseDamage;

    public Event<DamageReceiver, DamageResult> OnDealDamage;

    public bool TryDealDamage(GameObject gameObject, out DamageResult result)
    {
        if (((1 << gameObject.layer) & mask) != 0 && gameObject.TryGetComponent<DamageReceiver>(out var receiver))
        {
            result = receiver.DealDamage(baseDamage);

            OnDealDamage?.Invoke(receiver, result);

            return true;
        }

        result = default;
        return false;
    }
}
