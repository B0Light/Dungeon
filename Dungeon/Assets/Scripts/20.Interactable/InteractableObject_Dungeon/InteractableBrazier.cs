using UnityEngine;

public class InteractableBrazier : Interactable
{
    [SerializeField] private GameObject activateObject;
    private bool _isActivate = false;
    public override void Interact(PlayerManager player)
    {
        base.Interact(player);
        
        ActivateObject();
    }

    private void ActivateObject()
    {
        if(_isActivate) return;
        _isActivate = true;
        activateObject.SetActive(true);
    }
}
