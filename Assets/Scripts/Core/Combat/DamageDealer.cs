using UnityEngine;
using System.Collections;

public class DamageDealer : MonoBehaviour
{
    [SerializeField]
    private LayerMask mask;

    [SerializeField]
    [Min(0)]
    private int baseDamage;

    public bool TryDealDamage(GameObject gameObject)
    {
        if (gameObject.TryGetComponent<DamageReceiver>(out var receiver))
        {
            receiver.DealDamage(baseDamage);
            return true;
        }

        return false;
    }
}
