using UnityEngine;
using System.Collections.Generic;
using System;
using Unity.Cinemachine;

[Serializable]
public class TutorialDialogue
{
    public string tutorialStep;    // 튜토리얼 단계 (예: "움직임", "점프", "전투" 등)
    public string message;         // 해당 단계의 대화 내용
    public bool isCompleted;       // 해당 단계 완료 여부
}

public class TutorialNpc : Interactable
{
    [SerializeField] protected string npcName = "가이드";
    
    [Header("VCam")] 
    [SerializeField] protected CinemachineVirtualCameraBase vCam;
    
    [Header("Tutorial Dialogues")]
    [SerializeField] protected List<TutorialDialogue> tutorialDialogues = new List<TutorialDialogue>();
    
    [Header("Completion Messages")]
    [SerializeField] protected string allCompletedMessage = "모험을 떠나 더 나은 거점을 만들세요!";
    
    private void Start()
    {
        vCam.Priority = 0;
        
        // 기본 튜토리얼 대화들 설정 (예시)
        SetupDefaultTutorialDialogues();
    }
    
    private void SetupDefaultTutorialDialogues()
    {
        tutorialDialogues.Add(new TutorialDialogue
        {
            tutorialStep = "supply",
            message = "드롭쉽 옆 상자에서 보급품을 획득할 수 있어요.\n탐험을 떠나면 기존의 아이템은 제거되고 상자는 다시 채워질 꺼에요",
            isCompleted = false
        });
        
        tutorialDialogues.Add(new TutorialDialogue
        {
            tutorialStep = "build_00",
            message = "천막에서는 거점의 건물을 구입하고 건설할 수 있어요.\n또한 하루에 한번 거점을 개방하여 다른 고양이를 초대할 수 있어요.",
            isCompleted = false
        });
        
        tutorialDialogues.Add(new TutorialDialogue
        {
            tutorialStep = "build_01",
            message = "무기상점을 건설하세요. 무기가 없으면 적을 공격할 수 없어요.",
            isCompleted = false
        });
        
        tutorialDialogues.Add(new TutorialDialogue
        {
            tutorialStep = "build_02",
            message = "온천을 건설하세요. 복귀 후 휴식을 통해 체력을 회복하세요.",
            isCompleted = false
        });
        
        tutorialDialogues.Add(new TutorialDialogue
        {
            tutorialStep = "build_03",
            message = "컨테이너를 건설하세요. 사용하지 않은 아이템을 보관할 수 있어요!",
            isCompleted = false
        });
        
        tutorialDialogues.Add(new TutorialDialogue
        {
            tutorialStep = "visit",
            message = "방문객들이 오면 건물에서 재화를 얻을 수 있어요.\n" + 
                      "하루 한 번만 거점을 열 수 있고, 길이 연결되어야 해요.\n",
            isCompleted = false
        });
        
        tutorialDialogues.Add(new TutorialDialogue
        {
            tutorialStep = "build_04",
            message = "특전 관리자를 건설하면 새로운 액션을 해금할 수 있어요!",
            isCompleted = false
        });
    }
    
    public override void Interact(PlayerManager player)
    {
        base.Interact(player);
        
        // 각 단계 튜토리얼 진행여부 확인하기
        CheckTutorial();
        vCam.Priority = 20;
        GUIController.Instance.OpenDialogue(npcName, ResetInteraction);
        
        string messageToShow = GetCurrentTutorialMessage();
        GUIController.Instance.dialogueGUIManager.SetDialogueText(messageToShow);
        CompleteTutorialStep("supply");
    }
    
    private string GetCurrentTutorialMessage()
    {
        // 완료되지 않은 첫 번째 튜토리얼 찾기
        foreach (var tutorial in tutorialDialogues)
        {
            if (!tutorial.isCompleted)
            {
                return tutorial.message;
            }
        }
        
        // 모든 튜토리얼이 완료된 경우
        return allCompletedMessage;
    }
    
    public override void ResetInteraction()
    { 
        Debug.LogWarning("Reset Interaction");
        vCam.Priority = 0;
        
        PlayerInputManager.Instance.SetControlActive(true);
        
        base.ResetInteraction();
    }

    private void CheckTutorial()
    {
        if (WorldSaveGameManager.Instance.currentGameData.buildings.Count > 0)
        {
            CompleteTutorialStep("build_00");
        }
        
        foreach (var building in WorldSaveGameManager.Instance.currentGameData.buildings)
        {
            if (building.code == 310) // WeaponShop
            {
                CompleteTutorialStep("build_01");
            }
            if (building.code == 320) // Spa
            {
                CompleteTutorialStep("build_02");
            }
            if (building.code == 400) // Spa
            {
                CompleteTutorialStep("build_03");
            }
            if (building.code == 402) // PerkManager
            {
                CompleteTutorialStep("build_04");
            }
            
            if (WorldSaveGameManager.Instance.currentGameData.isVisitedToday)
            {
                CompleteTutorialStep("visit");
            }
        }
    }
    
    // 특정 튜토리얼 단계 완료 처리
    private void CompleteTutorialStep(string stepName)
    {
        var tutorial = tutorialDialogues.Find(t => t.tutorialStep == stepName);
        if (tutorial != null)
        {
            tutorial.isCompleted = true;
            Debug.Log($"튜토리얼 단계 '{stepName}' 완료!");
        }
    }
    
    // 모든 튜토리얼 완료 여부 확인
    public bool IsAllTutorialCompleted()
    {
        foreach (var tutorial in tutorialDialogues)
        {
            if (!tutorial.isCompleted)
                return false;
        }
        return true;
    }
    
    // 현재 진행 중인 튜토리얼 단계 가져오기
    public string GetCurrentTutorialStep()
    {
        foreach (var tutorial in tutorialDialogues)
        {
            if (!tutorial.isCompleted)
                return tutorial.tutorialStep;
        }
        return "완료";
    }
    
    // 튜토리얼 리셋 (테스트용)
    public void ResetAllTutorials()
    {
        foreach (var tutorial in tutorialDialogues)
        {
            tutorial.isCompleted = false;
        }
    }
    
    // 런타임에서 새로운 튜토리얼 단계 추가
    public void AddTutorialStep(string stepName, string message)
    {
        tutorialDialogues.Add(new TutorialDialogue
        {
            tutorialStep = stepName,
            message = message,
            isCompleted = false
        });
    }
}