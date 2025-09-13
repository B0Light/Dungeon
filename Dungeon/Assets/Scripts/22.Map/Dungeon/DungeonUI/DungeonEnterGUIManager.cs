using System.Collections.Generic;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DungeonEnterGUIManager : GUIComponent
{
    [Header("DungeonSelect")] 
    [SerializeField] private RectTransform dungeonSelectSlot;
    [SerializeField] private List<DungeonData> dungeonDataList;
    [SerializeField] private List<GameObject> dungeonSelectButtons;
    
    [SerializeField] private TextMeshProUGUI dungeonName;
    [SerializeField] private TextMeshProUGUI dungeonInfo;
    [SerializeField] private Image dungeonBackgroundImage;
    
    [SerializeField] private Button enterDungeonButton;
    [SerializeField] private GameObject available;
    [SerializeField] private GameObject disable;

    private Interactable _interactableObj;

    public override void CloseGUI()
    {
        base.CloseGUI();
        _interactableObj?.ResetInteraction();
    }

    public void InitDungeonEnterHUD(DungeonPlaceData dungeonPlaceData, Interactable interactable)
    {
        _interactableObj = interactable;
        
        PlayerInputManager.Instance.SetControlActive(false);
        
        if(dungeonPlaceData.dungeonDataList.Count == 0) return;

        ResetShelf();
        dungeonDataList.AddRange(dungeonPlaceData.dungeonDataList);

        for (var i = 0; i < dungeonDataList.Count; i++)
        {
            dungeonSelectButtons[i].SetActive(true);
            var spawnButton = dungeonSelectButtons[i].GetComponent<Button>();
    
            int capturedIndex = i;  
            spawnButton.onClick.AddListener(() => InitEntranceOfDungeon(dungeonDataList[capturedIndex]));
        }

        InitEntranceOfDungeon(dungeonPlaceData.dungeonDataList[0]);
    }
    
    private void ResetShelf()
    {
        foreach (GameObject button in dungeonSelectButtons)
        {
            button.SetActive(false);
        }
        dungeonDataList.Clear();
    }

    private void InitEntranceOfDungeon(DungeonData dungeonData)
    {
        dungeonName.text = dungeonData.dungeonName;
        dungeonInfo.text = dungeonData.GetFormattedInfo();
        dungeonBackgroundImage.sprite = dungeonData.dungeonInfoBackground;

        available.SetActive(false);
        disable.SetActive(false);
        enterDungeonButton.onClick.RemoveAllListeners();
        if (WorldSaveGameManager.Instance.currentGameData.availableDungeon[dungeonData.dungeonID])
        {
            enterDungeonButton.interactable = true;
            available.SetActive(true);
            enterDungeonButton.onClick.AddListener(() => EnterDungeon(dungeonData.dungeonSceneName));
        }
        else
        {
            disable.SetActive(true);
            enterDungeonButton.interactable = false;
        }
    }

    private void EnterDungeon(string dungeonSceneName)
    {
        GUIController.Instance.HandleEscape();
        WorldSceneChangeManager.Instance.LoadSceneAsync(dungeonSceneName);
    }
}
