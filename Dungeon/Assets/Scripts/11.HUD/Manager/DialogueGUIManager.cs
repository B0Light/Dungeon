using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[Serializable]
public class DialogueUIButton
{
    public Button button;
    public TextMeshProUGUI buttonText;
    public Image itemIcon;
}

public class DialogueGUIManager : GUIComponent
{
    [Header("UI 설정")]
    [SerializeField] private TextMeshProUGUI dialogueNpcName;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Transform buttonContainer;
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private Button closeButton;

    private List<DialogueUIButton> _dialogueUIButtons = new List<DialogueUIButton>();
    private UnityAction _closeAction;
    public void InitDialogue(string npcName, UnityAction closeAction)
    {
        dialogueNpcName.text = npcName;
        _closeAction = closeAction;
        
        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener(GUIController.Instance.HandleEscape);
    }

    public DialogueUIButton CreateDialogueButton(string dialogueComment, Sprite dialogueSprite)
    {
        GameObject buttonObj = Instantiate(buttonPrefab, buttonContainer);
        DialogueUIButton dialogueButton = new DialogueUIButton();
        
        // 버튼 컴포넌트들 가져오기
        dialogueButton.button = buttonObj.GetComponent<Button>();
        dialogueButton.buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
        dialogueButton.itemIcon = buttonObj.transform.Find("ItemIcon")?.GetComponent<Image>();
        
        dialogueButton.buttonText.text = dialogueComment;
        if(dialogueButton.itemIcon)
        {
            if (dialogueSprite != null)
            {
                dialogueButton.itemIcon.sprite = dialogueSprite;
                dialogueButton.itemIcon.gameObject.SetActive(true);
            }
            else
            {
                dialogueButton.itemIcon.gameObject.SetActive(false);
            }
        }
        
        _dialogueUIButtons.Add(dialogueButton);

        return dialogueButton;
    }
    
    
    public void SetDialogueText(string message)
    {
        if (dialogueText != null)
        {
            dialogueText.text = message;
        }
    }
    
    public void ClearButtons()
    {
        foreach (var button in _dialogueUIButtons)
        {
            if (button.button != null)
            {
                DestroyImmediate(button.button.gameObject);
            }
        }
        _dialogueUIButtons.Clear();
    }

    public override void CloseGUI()
    {
        ClearButtons();
        closeButton.onClick.RemoveAllListeners();
        
        _closeAction?.Invoke();
        _closeAction = null;
        
        base.CloseGUI();
    }
}
