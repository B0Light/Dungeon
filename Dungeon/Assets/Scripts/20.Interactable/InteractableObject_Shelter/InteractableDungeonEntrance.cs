using UnityEngine;
using UnityEngine.Serialization;

public class InteractableDungeonEntrance : Interactable
{
    [FormerlySerializedAs("_dungeonPlaceData")] [SerializeField] private DungeonInfoData dungeonInfoData;
    [SerializeField] private ParticleSystem vfx;
    public override void Interact(PlayerManager player)
    {
        base.Interact(player);
        WorldSaveGameManager.Instance.SaveGame();
        
        vfx?.Play();
        OpenHUD();
    }

    private void OpenHUD()
    {
        GUIController.Instance.OpenDungeonEntrance(dungeonInfoData, this);
    }

    public override void ResetInteraction()
    {
        base.ResetInteraction();
        vfx?.Stop();
    }
}
