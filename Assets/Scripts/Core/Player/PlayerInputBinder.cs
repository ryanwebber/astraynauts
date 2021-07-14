using UnityEngine;
using System.Collections;

public class PlayerInputBinder : MonoBehaviour
{
    public Event<IInputSource> OnAttachToInput;

    public void Bind(IInputSource source)
    {
        OnAttachToInput?.Invoke(source);
    }

    public void Bind(AttachableInputSource attachableInputSource)
    {
        attachableInputSource.RelativeAimObject = transform;
        Bind(attachableInputSource.MainSource);
    }
}
