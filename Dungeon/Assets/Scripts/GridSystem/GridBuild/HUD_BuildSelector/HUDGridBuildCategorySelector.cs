using UnityEngine;

public class HUDGridBuildCategorySelector : MonoBehaviour
{
    [SerializeField] private GameObject categoryPrefab;
    [SerializeField] private Transform selectButtonSlot;
    
    public void RefreshBuildingCategory()
    {
        RemoveAllChildren();
        foreach (var key in BuildingManager.Instance.unlockedBuildingByCategory.Keys)
        {
            if(key == TileCategory.Headquarter) continue;
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
