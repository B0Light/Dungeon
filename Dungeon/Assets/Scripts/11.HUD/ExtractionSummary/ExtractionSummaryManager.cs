using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ExtractionSummaryManager : GUIComponent
{
    [Header("Reword")]
    [SerializeField] private TextMeshProUGUI mapInfoText;
    [SerializeField] private TextMeshProUGUI lootValue;
    [SerializeField] private ItemGrid lootItemGrid;

    [Header("Combat")] 
    [SerializeField] private Transform killLogSlot;

    [Header("Boss")] 
    [SerializeField] private Image bossIcon;
    [SerializeField] private TextMeshProUGUI bossName;
    [SerializeField] private GameObject bossEliminatedMark;
    private bool _isBossEliminated = false;
    private int _bossId = 0;
 
    [Header("MostValuableItem")] 
    [SerializeField] private Image itemIcon;
    [SerializeField] private Image itemBackground;
    [SerializeField] private TextMeshProUGUI itemName;
    [SerializeField] private TextMeshProUGUI itemValue;

    [Header("Navigation Bar")]
    [SerializeField] private Button routeButton;
    [SerializeField] private Button combatButton;
    [SerializeField] private Button rewardButton;
    [SerializeField] private Button summaryButton;

    [Header("Panel")] 
    [SerializeField] private CanvasGroup routeCanvasGroup;
    [SerializeField] private CanvasGroup combatCanvasGroup;
    [SerializeField] private CanvasGroup rewardSummaryCanvasGroup;
    [SerializeField] private CanvasGroup summaryCanvasGroup;
    
    private List<CanvasGroup> _summaryPanels;
    private int _currentPanelIndex = 0;
    
    private AISpawnManager _aiSpawnManager;
    private WorldDatabase_Enemy _enemyDB;
    
    public void InitExtractionSummer()
    {
        _aiSpawnManager = AISpawnManager.Instance;
        _enemyDB = WorldDatabase_Enemy.Instance;
        /* Loot */
        WorldPlayerInventory.Instance.CalculateFinalLoot();
        
        mapInfoText.text = WorldSceneChangeManager.Instance.GetSceneName();
        lootValue.text = WorldPlayerInventory.Instance.TotalLootValue.ToString();

        SetMostValuableItem(WorldPlayerInventory.Instance.GetMostValuableItem());
        
        lootItemGrid.SetGrid(12,12, ConvertDictToRepeatedList(WorldPlayerInventory.Instance.finalItemDict));
        
        /* combat */
        SetEliminatedEnemy();
        
        /* Boss */
        BossEliminated();
        
        InitializePanels();
    }
    
    private void InitializePanels()
    {
        _summaryPanels = new List<CanvasGroup>
        {
            routeCanvasGroup,
            combatCanvasGroup,
            rewardSummaryCanvasGroup,
            summaryCanvasGroup
        };

        // 초기화: 모든 패널 비활성화
        foreach (var panel in _summaryPanels)
        {
            panel.alpha = 0;
            panel.interactable = false;
            panel.blocksRaycasts = false;
        }

        // 첫 번째 패널만 활성화
        ActivatePanel(0);
    }
    
    public void ActivatePanel(int index)
    {
        for (int i = 0; i < _summaryPanels.Count; i++)
        {
            bool isActive = (i == index);
            _summaryPanels[i].alpha = isActive ? 1 : 0;
            _summaryPanels[i].interactable = isActive;
            _summaryPanels[i].blocksRaycasts = isActive;
        }

        _currentPanelIndex = index;
    }

    public override void SelectNextGUI()
    {
        int nextIndex = _currentPanelIndex + 1;

        if (nextIndex < _summaryPanels.Count)
        {
            ActivatePanel(nextIndex);
        }
        else
        {
            ExitSummary(); // 모든 패널을 본 뒤 종료
        }
    }

    #region Reward

    private List<int> ConvertDictToRepeatedList(Dictionary<int, int> itemDict)
    {
        List<int> resultList = new List<int>();

        foreach (var kvp in itemDict)
        {
            int itemId = kvp.Key;
            int count = kvp.Value;

            for (int i = 0; i < count; i++)
            {
                resultList.Add(itemId);
            }
        }

        return resultList;
    }

    private void SetMostValuableItem(int itemId)
    {
        ItemInfo item = WorldDatabase_Item.Instance.GetItemByID(itemId);
        itemIcon.sprite = item.itemIcon;
        itemBackground.color = WorldDatabase_Item.Instance.GetItemColorByTier(item.itemTier);
        itemName.text = item.itemName;
        
        string color;
        int purchaseCost = item.purchaseCost;
        if (purchaseCost >= 100000)
            color = "#C71585"; 
        else if (purchaseCost >= 10000)
            color = "#8A2BE2"; 
        else if (purchaseCost >= 5000)
            color = "#00FFFF"; 
        else if (purchaseCost >= 1000)
            color = "#00FF00"; 
        else
            color = "#FFFFFF"; 

        itemValue.text = $"<color={color}>value : {purchaseCost}</color>";
    }

    #endregion

    #region CombatPanel

    private void SetEliminatedEnemy()
    {
        if(_aiSpawnManager == null || _enemyDB == null) return;
        
        foreach (var killLog in _aiSpawnManager.GetKillLog())
        {
            _enemyDB.CreateEnemyInfo(killLog.Key, killLog.Value, killLogSlot);

            if (killLog.Key % 10 == 0)
            {
                _isBossEliminated = true;
                _bossId = killLog.Key;
            }
        }
    }
    
    #endregion

    #region BossPanel

    private void BossEliminated()
    {
        if(_enemyDB == null) return;
        
        bossIcon.sprite = _enemyDB.GetIconById(_bossId);
        bossName.text = _enemyDB.GetNameById(_bossId);
        
        bossEliminatedMark.SetActive(_isBossEliminated && _bossId % 10 == 0);
    }

    #endregion


    #region Panel Control

    private void ExitSummary()
    {
        GUIController.Instance.HandleEscape();
        WorldSaveGameManager.Instance.SaveGame();
        WorldSceneChangeManager.Instance.LoadShelter();
    }

    #endregion
    
}
