using UnityEngine;

public class TentHeadquarter : RevenueFacilityTile_Shop
{
    public override void UpgradeTile()
    {
        base.UpgradeTile();

        WorldSaveGameManager.Instance.currentGameData.shelterLevel = this.level;
        BuildingManager.Instance.UpdateAvailableBuildings();
    }
}
