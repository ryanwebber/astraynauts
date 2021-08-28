using UnityEngine;
using System.Collections;

public class Height2D : MonoBehaviour
{
    [SerializeField]
    private Transform sprite;

    public float Height
    {
        get
        {
            if (sprite == null)
                return 0f;

            return sprite.localPosition.y;
        }

        set
        {
            if (sprite != null)
                sprite.localPosition = Vector3.up * value;
        }
    }
}
