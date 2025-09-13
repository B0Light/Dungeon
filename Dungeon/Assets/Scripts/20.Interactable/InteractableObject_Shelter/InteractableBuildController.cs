using System;

using System.Collections.Generic;
using UnityEngine;

public class InteractableBuildController : Interactable
{
    public override void Interact(PlayerManager player)
    {
        base.Interact(player);
        EnterController();
    }

    private void EnterController()
    {
        GUIController.Instance.ToggleMainGUI(false);
        PlayerInputManager.Instance.SetControlActive(false);
        BuildingManager.Instance.ToggleMainBuildHUD(true, this);
    }
}
