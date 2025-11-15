using UnityEngine;

public class TentHeadquarter : RevenueFacilityTile_Shop
{
    public override void UpgradeTile()
    {
        base.UpgradeTile();

        WorldSaveGameManager.Instance.currentGameData.shelterLevel = this.level;
        ShelterBuildHUDManager shelterBuildHUDManager = GridBuildHUDManager.Instance as ShelterBuildHUDManager; 
        if(shelterBuildHUDManager)
            shelterBuildHUDManager.UpdateAvailableBuildings();
    }
}
