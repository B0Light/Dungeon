using UnityEngine;
using UnityEngine.Events;

public class InteractableGateOpen : InteractableEvent
{
    public UnityEvent GateOpen { get; private set; }

    protected override void InteractionEvent()
    {
        throw new System.NotImplementedException();
    }
}
