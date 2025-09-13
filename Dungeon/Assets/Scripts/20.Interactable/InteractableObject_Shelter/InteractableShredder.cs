using System.Collections.Generic;
using UnityEngine;

public class InteractableShredder : Interactable
{
    private int _girdWidth = 3;
    private int _gridHeight = 4;
    private List<int> onCrusherItem = new List<int>();
    public override void Interact(PlayerManager player)
    {
        base.Interact(player);
        WorldSaveGameManager.Instance.SaveGame();
        OpenHUD();
    }

    private void OpenHUD()
    {
        GUIController.Instance.OpenShredder(_girdWidth, _gridHeight, onCrusherItem, this);
    }
    
    public override void SetToSpecificLevel(int level)
    {
        int[] widthValues = { 3, 4, 5, 6, 7, 8, 9, 9, 9, 9 };
        int[] heightValues = { 4, 5, 6, 7, 8, 8, 8, 9, 9, 9 };
    
        _girdWidth = widthValues[level];
        _gridHeight = heightValues[level];
    }
}
