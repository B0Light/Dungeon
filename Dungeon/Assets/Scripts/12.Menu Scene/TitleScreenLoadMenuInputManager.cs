using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class TitleScreenLoadMenuInputManager : MonoBehaviour
{
    private UI_CharacterSaveSlot _currentSelectSlot;

    [Header("Slot Detail")] 
    [SerializeField] private CanvasGroup detailPanel;
    [SerializeField] private TextMeshProUGUI selectSlotText;
    [SerializeField] private TextMeshProUGUI lastPlayTimeText;
    [SerializeField] private TextMeshProUGUI playTimeText;
    [SerializeField] private Button deleteSlotButton;
    [SerializeField] private Button playButton;

    private void OnEnable()
    {
        ToggleDetailPanel(false);
    }

    private string ConvertPlayTime(string isoTime)
    {
        if (DateTime.TryParse(isoTime, out DateTime dateTime))
        {
            return dateTime.ToString("yyyy-MM-dd HH:mm:ss");
        }
        else
        {
            Debug.LogError("Invalid ISO 8601 time format.");
            return "";
        }
    }

    public void SetSlot()
    {
        CharacterSlot selectSlot = TitleScreenManager.Instance.CurrentSelectedSlot;
        SaveGameData slot = WorldSaveGameManager.Instance.GetSaveGameData(selectSlot);

        WorldSaveGameManager.Instance.currentCharacterSlotBeingUsed = selectSlot;
        
        deleteSlotButton.onClick.RemoveAllListeners();
        playButton.onClick.RemoveAllListeners();

        ToggleDetailPanel(slot != null);
        
        if (slot == null)
        {
            selectSlotText.text = "None";
            lastPlayTimeText.text = "yyyy-MM-dd";
            playTimeText.text = "D-Day";
        }
        else
        {
            selectSlotText.text = slot.characterName;
            lastPlayTimeText.text = ConvertPlayTime(slot.lastPlayTime);
            playTimeText.text = "D-Day +" + slot.secondsPlayed;
            
            playButton.onClick.AddListener(()=>WorldSaveGameManager.Instance.LoadGame());
            deleteSlotButton.onClick.AddListener(()=>TitleScreenManager.Instance.OpenToDeleteCharacterSlot());
        }
    }

    private void ToggleDetailPanel(bool value)
    {
        detailPanel.alpha = value ? 1 : 0;
        detailPanel.interactable = value;
        detailPanel.blocksRaycasts = value;
    }
}

