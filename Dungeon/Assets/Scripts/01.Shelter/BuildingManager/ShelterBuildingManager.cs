
public class ShelterBuildingManager : GridBuildingManager
{
    public ShelterManager shelterManager;

    public override void ExitBuildHUD()
    {
        base.ExitBuildHUD();
        SaveGridData(); 
    }

    private void SaveGridData()
    {
        WorldSaveGameManager.Instance.currentGameData.buildings.Clear();
        ShelterBaseGridSystem shelterBaseGridSystem = BaseGridBuildSystem.Instance as ShelterBaseGridSystem;
        if(!shelterBaseGridSystem) return;
        foreach (var building in shelterBaseGridSystem.SaveBuildingDataList)
        {
            if (building != null)
            {
                WorldSaveGameManager.Instance.currentGameData.buildings.Add(building);
            }
        }
        WorldSaveGameManager.Instance.SaveGame();
    }
}
