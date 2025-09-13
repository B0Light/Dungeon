using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    public string interactableText; 
    protected Collider interactableCollider;   
    
    protected virtual void Awake()
    {
        //  CHECK IF IT'S NULL, IN SOME CASES YOU MAY WANT TO MANUALLY ASIGN A COLLIDER AS A CHILD OBJECT (DEPENDING ON INTERACTABLE)
        if (interactableCollider == null)
            interactableCollider = GetComponent<Collider>();
    }

    public virtual void Interact(PlayerManager player)
    {
        //  REMOVE THE INTERACTION FROM THE PLAYER
        interactableCollider.enabled = false;
        player.playerInteractionManager.RemoveInteractionFromList(this);
        GUIController.Instance.playerUIPopUpManager.CloseAllPopUpWindows();
    }
    
    private void OnTriggerExit(Collider other)
    {
        PlayerManager player = other.GetComponent<PlayerManager>();

        if (player != null)
        {
            player.playerInteractionManager.RemoveInteractionFromList(this);
            GUIController.Instance.playerUIPopUpManager.CloseAllPopUpWindows();
            
            ResetInteraction();
        }
    }
    
    public virtual void ResetInteraction()
    {
        interactableCollider.enabled = true;
    }
    
    public virtual void SetToSpecificLevel(int level) {}
}
