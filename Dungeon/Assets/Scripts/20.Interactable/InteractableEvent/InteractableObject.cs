using UnityEngine;

public class InteractableObject : Interactable
{
    [SerializeField] private InteractableEvent interactableEvent;
    public override void Interact(PlayerManager player)
    {
        base.Interact(player);
        interactableEvent.OnEvent();
    }
}
