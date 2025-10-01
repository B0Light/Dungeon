using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class BuildingManager : MonoBehaviour
{
    public static BuildingManager Instance { get; private set; }

    public HUDGridBuildCategorySelector gridBuildCategorySelector;
    public HUDGridBuildingSelector gridBuildingSelector;

    public ShelterManager shelterManager;

    public SerializableDictionary<TileType, HashSet<BuildObjData>> unlockedBuildingByCategory;

    [SerializeField] private CanvasGroup constructionCanvasGroup;
    [SerializeField] private CanvasGroup buildingSelectionCanvasGroup;
    [SerializeField] private CanvasGroup buildingPopupCanvasGroup;
    
    [Space(10)]
    [SerializeField] private HUD_BuildInfo buildInfoHUD;
    
    [Header("BuildCam")] 
    [SerializeField] private GridBuildCamera _gridBuildCamera;
    
    private Interactable _interactableObject;
    private RevenueFacilityTile _currentSelectTile = null;
    
    // 시간에 따라 상호작용 가능 여부 확인 

    private void Awake()
    {
        Instance = this;
        StartCoroutine(Init());
        
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
    
    private void Start()
    {
        ToggleMainBuildHUD(false);
        ToggleBuildPopUpHUD(false);
        ToggleBuildSelectionHUD(false);
    }

    // Interactable Build Controller 와 상호작용해서 HUD를 열때 사용 
    public void ToggleMainBuildHUD(bool isActive, Interactable interactable = null)
    {
        if (isActive && interactable != null)
        {
            _interactableObject = interactable;
        }
        else if (!isActive && _interactableObject != null)
        {
            _interactableObject.ResetInteraction();
            _interactableObject = null;
        }
        
        ToggleConstructionHUD(isActive);
        GridBuildingSystem.Instance.SetActive(isActive);
        if (isActive)
        {
            TurnOnGridBuildCamera();
        }
        else
        {
            TurnOffGridBuildCamera();
        }
    }

    public void ToggleBuildSelector()
    {
        bool isOpen = buildingSelectionCanvasGroup.interactable;

        gridBuildCategorySelector.RefreshBuildingCategory();
        ToggleBuildSelectionHUD(!isOpen);
    }

    public void ToggleConstructionHUD(bool isActive)
    {
        constructionCanvasGroup.alpha = isActive ? 1f : 0f;
        constructionCanvasGroup.blocksRaycasts = isActive;
        constructionCanvasGroup.interactable = isActive;
    }
    
    public void ToggleBuildSelectionHUD(bool isActive)
    {
        buildingSelectionCanvasGroup.alpha = isActive ? 1f : 0f;
        buildingSelectionCanvasGroup.blocksRaycasts = isActive;
        buildingSelectionCanvasGroup.interactable = isActive;
    }

    private void ToggleBuildPopUpHUD(bool isActive)
    {
        buildingPopupCanvasGroup.alpha = isActive ? 1f : 0f;
        buildingPopupCanvasGroup.blocksRaycasts = isActive;
        buildingPopupCanvasGroup.interactable = isActive;
    }

    public void OpenBuildPopUpHUD(RevenueFacilityTile revenueFacilityTile)
    {
        _currentSelectTile = revenueFacilityTile;
        _currentSelectTile.SelectObject(true);
        
        buildInfoHUD.Init(revenueFacilityTile);
        
        ToggleConstructionHUD(false);
        ToggleBuildPopUpHUD(true);
    }
    
    public void CloseBuildInfoHUD()
    {
        if (!_currentSelectTile) return;
        _currentSelectTile.SelectObject(false);
        _currentSelectTile = null;
        
        ToggleConstructionHUD(true);
        ToggleBuildPopUpHUD(false);
    }

    public void SelectCategory(TileType id)
    {
        GridBuildingSystem.Instance.SelectToBuild(null);
        StartCoroutine(gridBuildingSelector.InitBtnSlot(id));
    }

    public void RefreshCategory()
    {
        GridBuildingSystem.Instance.SelectToBuild(null);
        gridBuildingSelector.RefreshSlot();
    }

    private void TurnOnGridBuildCamera()
    {
        _gridBuildCamera.gameObject.SetActive(true);
        PlayerCameraController.Instance.TurnOffCamera();
    }
    
    private void TurnOffGridBuildCamera()
    {
        _gridBuildCamera.gameObject.SetActive(false);
        PlayerCameraController.Instance.TurnOnCamera();
    }

    public void ExitBuildHUD()
    {
        SaveGridData(); 
        
        GridBuildingSystem.Instance.SelectToBuild(null);
        GUIController.Instance.ToggleMainGUI(true);
        PlayerInputManager.Instance.SetControlActive(true);
        PlayerCameraController.Instance.TurnOnCamera();

        ToggleMainBuildHUD(false);
    }

    private void SaveGridData()
    {
        WorldSaveGameManager.Instance.currentGameData.buildings.Clear();
        foreach (var building in GridBuildingSystem.Instance.SaveBuildingDataList)
        {
            if (building != null)
            {
                WorldSaveGameManager.Instance.currentGameData.buildings.Add(building);
            }
        }
        WorldSaveGameManager.Instance.SaveGame();
    }
}
