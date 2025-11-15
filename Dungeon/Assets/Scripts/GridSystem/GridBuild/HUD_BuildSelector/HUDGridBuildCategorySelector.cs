using UnityEngine;

public class HUDGridBuildCategorySelector : MonoBehaviour
{
    [SerializeField] private GameObject categoryPrefab;
    [SerializeField] private Transform selectButtonSlot;
    
    public void RefreshBuildingCategory()
    {
        RemoveAllChildren();
        ShelterBuildHUDManager shelterBuildHUDManager = GridBuildHUDManager.Instance as ShelterBuildHUDManager;
        if(!shelterBuildHUDManager) return;
        foreach (var key in shelterBuildHUDManager.unlockedBuildingByCategory.Keys)
        {
            if(key == TileType.Headquarter) continue;
            GameObject instanceBtnObj = Instantiate(categoryPrefab, selectButtonSlot);
            instanceBtnObj.GetComponent<HUDGridBuildingCategoryUnit>()?.InitButton(key);
        }
    }
    
    private void RemoveAllChildren()
    {
        if (selectButtonSlot == null)
        {
            return;
        }

        for (int i = selectButtonSlot.childCount - 1; i >= 0; i--)
        {
            Destroy(selectButtonSlot.GetChild(i).gameObject);
        }
    }
}
