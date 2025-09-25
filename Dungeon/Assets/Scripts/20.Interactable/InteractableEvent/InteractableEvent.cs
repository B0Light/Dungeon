using UnityEngine;

public abstract class InteractableEvent : ScriptableObject
{
    public void OnEvent()
    {
        InteractionEvent();
    }

    protected abstract void InteractionEvent();
}
