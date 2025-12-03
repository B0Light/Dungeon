using UnityEngine;

public class DungeonCoreHUDManager : GridBuildHUDManager
{
    [SerializeField] private DungeonManager dungeonManager;

    public override void ExitBuildHUD()
    {
        base.ExitBuildHUD();
        dungeonManager.StartNavMeshRenewal();
    }
}
