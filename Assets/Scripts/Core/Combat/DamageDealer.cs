using UnityEngine;
using System.Collections;

public class DamageDealer : MonoBehaviour
{
    [SerializeField]
    private LayerMask mask;

    [SerializeField]
    [Min(0)]
    private int baseDamage;

    public bool TryDealDamage(GameObject gameObject, out DamageResult result)
    {
        if (((1 << gameObject.layer) & mask) != 0 && gameObject.TryGetComponent<DamageReceiver>(out var receiver))
        {
            result = receiver.DealDamage(baseDamage);
            return true;
        }

        result = default;
        return false;
    }
}
