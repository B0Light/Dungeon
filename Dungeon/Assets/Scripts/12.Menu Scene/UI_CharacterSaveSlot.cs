using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class UI_CharacterSaveSlot : MonoBehaviour
{
    [Header("Game Slot")]
    public CharacterSlot characterSlot;

    [Header("Character Info")]
    public TextMeshProUGUI characterName;
    public TextMeshProUGUI lastPlayedTime;
    public TextMeshProUGUI timePlayed;

    private void OnEnable()
    {
        LoadSaveSlots();
    }

    private void LoadSaveSlots()
    {
        SaveFileDataWriter saveFileWriter = new SaveFileDataWriter();
        saveFileWriter.saveDataDirectoryPath = Application.persistentDataPath;
    
        saveFileWriter.saveFileName = WorldSaveGameManager.Instance.DecideCharacterFileNameBasedOnCharacterSlotBeingUsed(characterSlot);
    
        if (!saveFileWriter.CheckToSeeIfFileExists())
        {
            gameObject.SetActive(false);
            return;
        }

        SetSlotInfo(WorldSaveGameManager.Instance.GetSaveGameData(characterSlot));
    }

    public void LoadGameFromCharacterSlot()
    {
        WorldSaveGameManager.Instance.currentCharacterSlotBeingUsed = characterSlot;
        WorldSaveGameManager.Instance.LoadGame();
    }

    public void SelectCurrentSlot()
    {
        TitleScreenManager.Instance.SelectCharacterSlot(characterSlot);
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

    private void SetSlotInfo(SaveGameData slot)
    {
        characterName.text = slot.characterName;
        
        lastPlayedTime.text = ConvertPlayTime(slot.lastPlayTime);
        timePlayed.text = "D-Day +" + slot.secondsPlayed;
    }
}

