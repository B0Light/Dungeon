using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;

public class AdvancedDungeonManager : InteractableNpc
{
    [Header("던전 설정")]
    [SerializeField] private DungeonPlaceData dungeons;
    
    [Header("상호작용 모드")]
    [SerializeField] private InteractionMode mode = InteractionMode.SingleDungeon;
    [SerializeField] private int targetDungeonIndex = 0;

    [Header("NPC 대화 설정")] 
    private string insufficientItemsMessage = "아직은 그곳에 갈 수 없어";
    private string alreadyUnlockedMessage = "이미 모든 곳을 탐험할 수 있어";
    private string unlockMessage = "이제 그곳에 갈 수 있어\n조심하게나 이곳의 생물은 자네가 알던 생물과는 다르네";
    
    private enum InteractionMode
    {
        SingleDungeon,      // 특정 던전 하나만 관리
        MultipleDungeons,   // 여러 던전 선택 UI 표시
        Sequential         // 순차적으로 던전 해제
    }

    public override void Interact(PlayerManager player)
    {
        base.Interact(player);
        
        switch (mode)
        {
            case InteractionMode.SingleDungeon:
                HandleSingleDungeonUI();
                break;
            case InteractionMode.MultipleDungeons:
                HandleMultipleDungeonsUI();
                break;
            case InteractionMode.Sequential:
                HandleSequentialUI();
                break;
        }
    }

    private void HandleSingleDungeonUI()
    {
        if (IsValidDungeonIndex(targetDungeonIndex))
        {
            var dungeon = dungeons.dungeonDataList[targetDungeonIndex];
            
            if (IsDungeonUnlocked(dungeon))
            {
                GUIController.Instance.dialogueGUIManager.SetDialogueText($"{dungeon.dungeonName}은 언제든 갈 수 있다네");
                GUIController.Instance.dialogueGUIManager.ClearButtons();
            }
            else
            {
                GUIController.Instance.dialogueGUIManager.SetDialogueText(interactionMsg);
                CreateSingleDungeonButton(dungeon);
            }
        }
    }

    private void HandleMultipleDungeonsUI()
    {
        var availableDungeons = GetAvailableDungeons();
        
        if (availableDungeons.Count == 0)
        {
            GUIController.Instance.dialogueGUIManager.SetDialogueText(HasAnyUnlockedDungeons() ? alreadyUnlockedMessage : interactionMsg);
            GUIController.Instance.dialogueGUIManager.ClearButtons();
        }
        else
        {
            GUIController.Instance.dialogueGUIManager.SetDialogueText(interactionMsg);
            CreateMultipleDungeonButtons();
        }
    }

    private void HandleSequentialUI()
    {
        var nextDungeon = GetNextSequentialDungeon();
        
        if (nextDungeon == null)
        {
            GUIController.Instance.dialogueGUIManager.SetDialogueText(alreadyUnlockedMessage);
            GUIController.Instance.dialogueGUIManager.ClearButtons();
        }
        else
        {
            GUIController.Instance.dialogueGUIManager.SetDialogueText(interactionMsg);
            CreateSingleDungeonButton(nextDungeon);
        }
    }

    private void CreateSingleDungeonButton(DungeonData dungeon)
    {
        GUIController.Instance.dialogueGUIManager.ClearButtons();
        
        if (CanUnlockDungeon(dungeon))
        {
            CreateDungeonButton(dungeon);
        }
    }

    private void CreateMultipleDungeonButtons()
    {
        GUIController.Instance.dialogueGUIManager.ClearButtons();
        
        foreach (var dungeon in dungeons.dungeonDataList)
        {
            if (CanUnlockDungeon(dungeon))
            {
                CreateDungeonButton(dungeon);
            }
        }
    }

    private void CreateDungeonButton(DungeonData dungeon)
    {
        string dialogueComment;
        Sprite dialogueIcon;
        // 버튼 텍스트 설정
        var requiredItem = WorldDatabase_Item.Instance.GetItemByID(dungeon.dungeonKey);
        if (requiredItem && requiredItem.itemCode != 0)
        {
            dialogueComment = $"{requiredItem.itemName}을 건넨다";
            dialogueIcon = requiredItem.itemIcon;
        }
        else
        {
            dialogueComment = $"{dungeon.dungeonName}에 가고 싶어";
            dialogueIcon = null;
        }

        var dungeonButton = GUIController.Instance.dialogueGUIManager.CreateDialogueButton(dialogueComment, dialogueIcon);


        if (requiredItem.itemCode == 0 || CheckRequirements(dungeon))
        {
            dungeonButton.button.interactable = true;
            dungeonButton.button.onClick.AddListener(() => OnDungeonButtonClick(dungeon));
        }
        else
        {
            dungeonButton.button.onClick.RemoveAllListeners();
            dungeonButton.button.interactable = false;
        }
    }

    private void OnDungeonButtonClick(DungeonData dungeon)
    {
        TryUnlockDungeon(dungeon);
    }

    private void TryUnlockDungeon(DungeonData dungeonAvailable)
    {
        if (IsDungeonUnlocked(dungeonAvailable))
        {
            GUIController.Instance.dialogueGUIManager.SetDialogueText($"{dungeonAvailable.dungeonName}은(는) 이미 해제된 던전입니다.");
            return;
        }

        if (dungeonAvailable.dungeonKey == 0 || CheckRequirements(dungeonAvailable))
        {
            ConsumeRequirements(dungeonAvailable);
            UnlockDungeon(dungeonAvailable);
            GUIController.Instance.dialogueGUIManager.SetDialogueText(unlockMessage);
            
            // UI 새로고침
            Invoke(nameof(RefreshUI), 5f);
        }
        else
        {
            GUIController.Instance.dialogueGUIManager.SetDialogueText(insufficientItemsMessage);
        }
    }

    private bool CheckRequirements(DungeonData dungeonAvailable)
    {
        return WorldPlayerInventory.Instance.CheckItemInInventory(dungeonAvailable.dungeonKey);
    }

    private void ConsumeRequirements(DungeonData dungeonAvailable)
    {
        var item = WorldDatabase_Item.Instance.GetItemByID(dungeonAvailable.dungeonKey);
        WorldPlayerInventory.Instance.RemoveItemInInventory(dungeonAvailable.dungeonKey);
    }

    private void UnlockDungeon(DungeonData dungeonAvailable)
    {
        WorldSaveGameManager.Instance.currentGameData.availableDungeon[dungeonAvailable.dungeonID] = true;
        WorldSaveGameManager.Instance.SaveGame();
        
        
        Debug.Log($"{dungeonAvailable.dungeonName} 던전이 해제되었습니다!");
    }

    private bool IsDungeonUnlocked(DungeonData dungeonAvailable)
    {
        return WorldSaveGameManager.Instance.currentGameData.availableDungeon[dungeonAvailable.dungeonID];
    }

    private bool CanUnlockDungeon(DungeonData dungeon)
    {
        return !IsDungeonUnlocked(dungeon);
    }

    private List<DungeonData> GetAvailableDungeons()
    {
        return dungeons.dungeonDataList.Where(d => !IsDungeonUnlocked(d)).ToList();
    }

    private DungeonData GetNextSequentialDungeon()
    {
        return dungeons.dungeonDataList.FirstOrDefault(d => !IsDungeonUnlocked(d));
    }

    private bool IsValidDungeonIndex(int index)
    {
        return index >= 0 && index < dungeons.dungeonDataList.Count;
    }

    private bool HasAnyUnlockedDungeons()
    {
        return dungeons.dungeonDataList.Any(IsDungeonUnlocked);
    }

    private void RefreshUI()
    {
        // 현재 모드에 따라 UI 다시 생성
        switch (mode)
        {
            case InteractionMode.SingleDungeon:
                HandleSingleDungeonUI();
                break;
            case InteractionMode.MultipleDungeons:
                HandleMultipleDungeonsUI();
                break;
            case InteractionMode.Sequential:
                HandleSequentialUI();
                break;
        }
    }
}