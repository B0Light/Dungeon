using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class DialogueLine
{
    public string speakerName;
    public string dialogueText;
    public AudioClip voiceClip; // 대사 음성
    public Sprite speakerImage; // 대화하는 캐릭터 이미지
}

[System.Serializable]
public class DialogueSet
{
    public string dialogueId; // 대화 세트의 고유 식별자
    public List<DialogueLine> lines;
}

public class DialogueManager : MonoBehaviour
{
    // 현재 대화 관리
    private List<DialogueLine> currentDialogue;
    private int currentLineIndex = 0;

    // 대화 세트 관리
    public List<DialogueSet> allDialogueSets;

    public void StartDialogue(string dialogueId)
    {
        // 특정 대화 세트 시작
        DialogueSet selectedDialogue = allDialogueSets.Find(set => set.dialogueId == dialogueId);
        
        if (selectedDialogue != null)
        {
            currentDialogue = selectedDialogue.lines;
            currentLineIndex = 0;
            DisplayNextLine();
        }
    }

    public void DisplayNextLine()
    {
        if (currentDialogue == null || currentLineIndex >= currentDialogue.Count)
        {
            EndDialogue();
            return;
        }
        
        DialogueLine currentLine = currentDialogue[currentLineIndex];

        GUIController.Instance.OpenDialogue(currentLine.dialogueText, EndDialogue);
        currentLineIndex++;
    }

    private void EndDialogue()
    {
        // 대화 종료 시 UI 초기화
        currentDialogue = null;
        currentLineIndex = 0;
    }
}