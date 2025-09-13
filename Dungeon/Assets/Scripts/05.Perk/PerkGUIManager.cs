using System;
using System.Collections.Generic;
using UnityEngine;

public class PerkGUIManager : GUIComponent
{
    [SerializeField] private PerkTooltip perkTooltip;

    [Header("Ref")] 
    [SerializeField] private GameObject perkNodeRef;
    [SerializeField] private GameObject connector;

    [Header("Slot")] 
    [SerializeField] private List<Transform> perkSlotList = new List<Transform>();

    private PlayerManager _playerManager;
    private Interactable _interactableObj;
    
    private void Start()
    {
        perkTooltip.ToggleTooltip(false);
    }

    public void OpenPerkManager(PlayerManager player, Interactable interactable)
    {
        _playerManager = player;
        _interactableObj = interactable;
        RemoveAllChildren();
        foreach (var perkId in WorldDatabase_Perk.Instance.PerkDict.Keys)
        {
            if(perkId % 10 != 0) continue;
            int category = perkId / 100;
            GameObject perkNode = Instantiate(perkNodeRef, perkSlotList[category - 1]);
            StartCoroutine(perkNode.GetComponentInChildren<MainPerkNode>()?.Init(perkId, this));
            if (WorldDatabase_Perk.Instance.MainPerkDict.ContainsKey(perkId))
                Instantiate(connector, perkSlotList[category - 1]);
        }
    }

    private void RemoveAllChildren()
    {
        foreach (var slot in perkSlotList)
        {
            foreach (Transform child in slot)
            {
                Destroy(child.gameObject);
            }
        }
    }

    public override void CloseGUI()
    {
        base.CloseGUI();
        WorldSaveGameManager.Instance.SaveGame();
        _interactableObj?.ResetInteraction();
        _playerManager.LoadPerkData(ref WorldSaveGameManager.Instance.currentGameData);
    }
    
    public void ShowTooltip(PerkNode mainPerkNode)
    {
        perkTooltip.Init(mainPerkNode.GetPerk());
        if(perkTooltip.transform is RectTransform rect)
            rect.position = GetTooltipPosition(mainPerkNode);
        perkTooltip.ToggleTooltip(true);
        
    }

    private Vector2 GetTooltipPosition(PerkNode perkNode)
    {
        RectTransform rect = perkNode.transform as RectTransform;
        RectTransform tooltipRect = perkTooltip.transform as RectTransform;
        if(!rect || !tooltipRect) return Vector2.zero;
        
        // 현재 선택된 아이템의 좌표
        Vector2 tooltipPosition = rect.transform.position;

        // 화면의 절반 좌표 계산
        float screenWidthHalf = Screen.width / 2f;
        float screenHeightHalf = Screen.height / 2f;
        
        // 기본 우측 상단
        tooltipPosition.x += rect.rect.width/2;
        tooltipPosition.y += rect.rect.height/2;
        
        if (tooltipPosition.x > screenWidthHalf)
        {
            // 아이템이 화면 오른쪽에 있으면 툴팁을 왼쪽으로 이동
            tooltipPosition.x -= (rect.rect.width + tooltipRect.rect.width);
        }
        if (tooltipPosition.y < screenHeightHalf)
        {
            // 아이템이 화면 아래쪽에 있으면 툴팁을 위로 이동
            tooltipPosition.y += (tooltipRect.rect.height - rect.rect.height) * Mathf.Clamp01((Screen.height - tooltipPosition.y ) / Screen.height);
        }

        return tooltipPosition;
    }

    public void HideTooltip()
    {
        perkTooltip.ToggleTooltip(false);
    }

    public void ActiveEffect()
    {
        GlowWireFrame(true);
    }
    
    public void RefundEffect()
    {
        GlowWireFrame(false);
    }

    private void GlowWireFrame(bool isAcquire)
    {
        InteractablePerkController perkController = _interactableObj as InteractablePerkController;
        if(!perkController) return;
        
        perkController.wireframeShader.ShowWireFrameMat(isAcquire);
    }
}
