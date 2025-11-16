using UnityEngine;

public class DungeonTurretBuildUIManager : MonoBehaviour
{
    [SerializeField] private Transform leftSlot;
    [SerializeField] private Transform rightSlot;
    [SerializeField] private GameObject turretPrefab;

    private void Start()
    {
        InitBtnSlot();
    }

    private void InitBtnSlot()
    {
        BaseGridBuildSystem.Instance.SelectToBuild(null);
        
        CleanupSlot();

        int slotId = 0;
        foreach (var turretId in WorldSaveGameManager.Instance.currentGameData.dungeonTurretList)
        {
            GameObject instanceBtnObj = Instantiate(turretPrefab, slotId / 2 == 0 ? leftSlot : rightSlot);
            ShopShelfItem_Building btnUnit = instanceBtnObj.GetComponent<ShopShelfItem_Building>();
            btnUnit.Init(WorldDatabase_Build.Instance.GetBuildingByID(turretId));
            slotId++;
        }
    }
    
    private void CleanupSlot()
    {
        for (int i = leftSlot.childCount - 1; i >= 0; i--)
        {
            Destroy(leftSlot.GetChild(i).gameObject);
        }
        
        for (int i = rightSlot.childCount - 1; i >= 0; i--)
        {
            Destroy(rightSlot.GetChild(i).gameObject);
        }
    }
}
