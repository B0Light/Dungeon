using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableForge : Interactable
{
    public override void Interact(PlayerManager player)
    {
        base.Interact(player);
        WorldSaveGameManager.Instance.SaveGame();
        EnterForge();
    }
    
    private void EnterForge()
    {
        InputHandlerManager.Instance.SetInputMode(InputMode.OpenUI);
        GUIController.Instance.OpenForge(this);
    }
}
