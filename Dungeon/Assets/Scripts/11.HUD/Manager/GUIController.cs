using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI.Extensions;


public class GUIController : Singleton<GUIController>
{
    public SettingGUIManager settingGUIManager;
    
    [HideInInspector] public PlayerUIHudManager playerUIHudManager;
    [HideInInspector] public PlayerUIPopUpManager playerUIPopUpManager;
    [HideInInspector] public InventoryGUIManager inventoryGUIManager;
    [HideInInspector] public ItemShopUIManager itemShopUIManager;
    [HideInInspector] public DungeonEnterGUIManager dungeonEnterGUIManager;
    [HideInInspector] public DialogueGUIManager dialogueGUIManager;
    [HideInInspector] public UI_InteractionCountDown interactionCountDown;
    [HideInInspector] public ExtractionSummaryManager extractionSummaryManager;
    
    [HideInInspector] public MapGUIManager mapGUIManager;
    [HideInInspector] public PerkGUIManager perkGUIManager;
    [SerializeField] private CanvasGroup cashCanvasGroup;

    [ReadOnly] public bool popUpWindowIsOpen = false;
    [ReadOnly] public GUIComponent currentOpenGUI;

    private CanvasGroup _canvasGroup;
    private bool _activeMainHud = true;

    protected override void Awake()
    {
        base.Awake();
        playerUIHudManager = GetComponentInChildren<PlayerUIHudManager>();
        playerUIPopUpManager = GetComponentInChildren<PlayerUIPopUpManager>();
        inventoryGUIManager = GetComponentInChildren<InventoryGUIManager>();
        itemShopUIManager = GetComponentInChildren<ItemShopUIManager>();
        dungeonEnterGUIManager = GetComponentInChildren<DungeonEnterGUIManager>();
        dialogueGUIManager = GetComponentInChildren<DialogueGUIManager>();
        interactionCountDown = GetComponentInChildren<UI_InteractionCountDown>();
        extractionSummaryManager = GetComponentInChildren<ExtractionSummaryManager>();
        mapGUIManager = GetComponentInChildren<MapGUIManager>();
        perkGUIManager = GetComponentInChildren<PerkGUIManager>();

        _canvasGroup = GetComponent<CanvasGroup>();
    }
    
    public void HandleEscape()
    {
        if(_activeMainHud == false) return;
        if (!TryCloseActiveUI())
        {
            OpenPauseMenu();
        }
    }

    private bool TryCloseActiveUI()
    {
        if (currentOpenGUI == null)
            return false;

        CloseGUI();
        return true;
    }

    private void OpenPauseMenu()
    {
        OpenGUI(settingGUIManager);
        settingGUIManager.OpenDisplaySetter();
    }

    
    public void HandleTab()
    {
        if(_activeMainHud == false) return;
        switch (currentOpenGUI)
        {
            case InventoryGUIManager:
            case null:
                ToggleInventory();
                break;
            default:
                TryCloseActiveUI();
                break;
        }
    }

    public void HandleNextGUI()
    {
        if(currentOpenGUI == null) return;

        currentOpenGUI.SelectNextGUI();
    }

    #region Control GUI

    private void OpenGUI(GUIComponent newGUI)
    {
        if (currentOpenGUI == newGUI)
            return; // 이미 열려있는 경우 아무것도 안 함

        CloseCurrentGUI();

        currentOpenGUI = newGUI;
        currentOpenGUI?.OpenGUI();
    }

    public void CloseGUI()
    {
        CloseCurrentGUI();
        currentOpenGUI = null;

        cashCanvasGroup.alpha = 0;
    }

    private void CloseCurrentGUI() => currentOpenGUI?.CloseGUI();

    #endregion
    
    public void OpenShop(List<ItemInfo> items, Interactable interactableObject, bool isMaster = false)
    {
        OpenGUI(itemShopUIManager);
        itemShopUIManager.OpenShop(items, interactableObject, isMaster);
    }

    private void ToggleInventory()
    {
        if (inventoryGUIManager.IsOpen)
            TryCloseActiveUI();
        else
            OpenInventory();
    }

    private void OpenInventory()
    {
        OpenGUI(inventoryGUIManager);
    }

    public void OpenInteractableBox(int width, int height, List<int> itemIdList, Interactable interactable)
    {
        OpenGUI(inventoryGUIManager);
        inventoryGUIManager.OpenInteractionInventory(false, width, height, itemIdList, interactable);
    }
    
    public void OpenShareBox(int width, int height, List<int> itemIdList, Interactable interactable)
    {
        OpenGUI(inventoryGUIManager);
        inventoryGUIManager.OpenInteractionInventory(true, width, height, itemIdList, interactable);
    }

    public void OpenDungeonEntrance(DungeonPlaceData dungeonPlaceData, Interactable interactable)
    {
        OpenGUI(dungeonEnterGUIManager);
        dungeonEnterGUIManager.InitDungeonEnterHUD(dungeonPlaceData, interactable);
    }

    public void OpenForge(Interactable interactable)
    {
        OpenGUI(inventoryGUIManager);
        inventoryGUIManager.OpenForge(interactable);
    }

    public void OpenDialogue(string npcName, UnityAction closeAction)
    {
        OpenGUI(dialogueGUIManager);
        dialogueGUIManager.InitDialogue(npcName, closeAction);
    }

    public void OpenShredder(int width, int height, List<int> itemIdList, Interactable interactable)
    {
        OpenGUI(inventoryGUIManager);
        inventoryGUIManager.OpenInteractionShredder(width, height, itemIdList, interactable);
        
        //cashCanvasGroup.alpha = 1;
    }

    public void OpenPerkManager(PlayerManager player, Interactable interactable)
    {
        OpenGUI(perkGUIManager);
        perkGUIManager.OpenPerkManager(player, interactable);
    }

    public void OpenMap()
    {
        OpenGUI(mapGUIManager);
    }
    
    public void WaitToInteraction(Action action)
    {
        interactionCountDown.Interaction(action);
    }

    public void OpenExtractionSummery()
    { 
        OpenGUI(extractionSummaryManager);
        extractionSummaryManager.InitExtractionSummer();
    }

    public void ToggleMainGUI(bool value)
    {
        _canvasGroup.alpha = value ? 1 : 0;
        _canvasGroup.interactable = value;
        _canvasGroup.blocksRaycasts = value;
        _activeMainHud = value;
    }
}

