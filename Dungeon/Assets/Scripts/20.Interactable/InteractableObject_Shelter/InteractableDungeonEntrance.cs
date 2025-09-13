using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableDungeonEntrance : Interactable
{
    [SerializeField] private DungeonPlaceData _dungeonPlaceData;
    public override void Interact(PlayerManager player)
    {
        base.Interact(player);
        WorldSaveGameManager.Instance.SaveGame();
        OpenHUD();
    }

    private void OpenHUD()
    {
        GUIController.Instance.OpenDungeonEntrance(_dungeonPlaceData, this);
    }
}
