using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShelterBuildHUDManager : GridBuildHUDManager
{
    public ShelterManager shelterManager;
    
    public HUDGridBuildCategorySelector gridBuildCategorySelector;
    public HUDGridBuildingSelector gridBuildingSelector;
    
    [Header("Canvas Group")]
    [SerializeField] private CanvasGroup buildingSelectionCanvasGroup;
    
    public SerializableDictionary<TileType, HashSet<BuildObjData>> unlockedBuildingByCategory;
    
    protected override void Awake()
    {
        base.Awake();
        StartCoroutine(Init());
    }

    protected override void Start()
    {
        base.Start();
        ToggleBuildSelectionHUD(false);
    }

    private IEnumerator Init()
    {
        yield return WaitForDataLoad();
        
        InitBuildingCategory();
        UpdateAvailableBuildings();
    }
    
    private IEnumerator WaitForDataLoad()
    {
        // 데이터가 로드될 때까지 대기
        while (!WorldDatabase_Build.Instance.IsDataLoaded)
        {
            yield return null; // 한 프레임 대기
        }
    }
    
    private void InitBuildingCategory()
    {
        foreach (TileType tileCategory in Enum.GetValues(typeof(TileType)))
        {
            if(tileCategory == TileType.None) return;
            if(unlockedBuildingByCategory.ContainsKey(tileCategory) == false)
                unlockedBuildingByCategory.Add(tileCategory, new HashSet<BuildObjData>());
        }
    }
    
    public void UpdateAvailableBuildings()
    {
        int curShelterLevel = WorldSaveGameManager.Instance.currentGameData.shelterLevel;

        foreach (var buildObjData in WorldDatabase_Build.Instance.GetBuildingsUpToTierReadOnly((ItemTier)curShelterLevel))
        {
            UpdateCategory(buildObjData);
        }
        gridBuildCategorySelector.RefreshBuildingCategory();
    }
    
    private void UpdateCategory(BuildObjData buildObjData)
    {
        TileType tileCategory = buildObjData.GetTileType(); 
        
        if(unlockedBuildingByCategory.ContainsKey(tileCategory) == false)
            unlockedBuildingByCategory.Add(tileCategory, new HashSet<BuildObjData>());
        
        unlockedBuildingByCategory[tileCategory].Add(buildObjData);
    }
    
    public void ToggleBuildSelector()
    {
        bool isOpen = buildingSelectionCanvasGroup.interactable;

        gridBuildCategorySelector.RefreshBuildingCategory();
        ToggleBuildSelectionHUD(!isOpen);
    }
    
    public void ToggleBuildSelectionHUD(bool isActive)
    {
        buildingSelectionCanvasGroup.alpha = isActive ? 1f : 0f;
        buildingSelectionCanvasGroup.blocksRaycasts = isActive;
        buildingSelectionCanvasGroup.interactable = isActive;
    }
    
    public void SelectCategory(TileType id)
    {
        BaseGridBuildSystem.Instance.SelectToBuild(null);
        StartCoroutine(gridBuildingSelector.InitBtnSlot(id));
    }

    public void RefreshCategory()
    {
        BaseGridBuildSystem.Instance.SelectToBuild(null);
        gridBuildingSelector.RefreshSlot();
    }

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
