using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public enum CharacterSlot
{
    CharacterSlot01,
    CharacterSlot02,
    CharacterSlot03,
    CharacterSlot04,
    CharacterSlot05,
    NoSlot
}

public class WorldSaveGameManager : Singleton<WorldSaveGameManager>
{
    private SaveFileDataWriter _saveFileDataWriter;
    
    [Header("Current Data")]
    public CharacterSlot currentCharacterSlotBeingUsed;
    public SaveGameData currentGameData;
    private string _saveFileName;

    [Header("Character Slots")]
    public List<SaveGameData> characterSlots = new List<SaveGameData>();
    private readonly int _maxCharacterSlots = 5; // 슬롯 수를 쉽게 조정 가능

    private void Start()
    {
        // 슬롯 데이터 초기화
        for (int i = 0; i < _maxCharacterSlots; i++)
        {
            characterSlots.Add(null);
        }
        
        LoadAllCharacterProfiles();
    }

    public string DecideCharacterFileNameBasedOnCharacterSlotBeingUsed(CharacterSlot characterSlot)
    {
        // Enum 값을 정수로 변환하여 사용
        var slotIndex = (int)characterSlot;
        return $"characterSlot{slotIndex + 1:00}";
    }

    public void AttemptToCreateNewGame(string playerName)
    {
        _saveFileDataWriter = new SaveFileDataWriter();
        _saveFileDataWriter.saveDataDirectoryPath = Application.persistentDataPath;

        foreach (CharacterSlot characterSlot in Enum.GetValues(typeof(CharacterSlot)))
        {
            // NO_SLOT은 건너뛰기
            if (characterSlot == CharacterSlot.NoSlot) continue;
            
            _saveFileDataWriter.saveFileName = DecideCharacterFileNameBasedOnCharacterSlotBeingUsed(characterSlot);
            if (!_saveFileDataWriter.CheckToSeeIfFileExists())
            {
                // IF THIS PROFILE SLOT IS NOT TAKEN, MAKE A NEW ONE USING THIS SLOT
                currentCharacterSlotBeingUsed = characterSlot;
                currentGameData = new SaveGameData();
                //Debug.Log($"새 게임 생성: 슬롯 {(int)characterSlot}");
                NewGame(playerName);
                return;
            }
        }
        
        // IF THERE ARE NO FREE SLOTS, NOTIFY THE PLAYER
        //Debug.LogWarning("사용 가능한 슬롯이 없습니다.");
        TitleScreenManager.Instance.OpenNoFreeCharacterSlotsPopUp();
    }

    private void NewGame(string playerName)
    {
        SetDefaultGameData(playerName);
        StartCoroutine(LoadWorldScene());
    }

    private void SetDefaultGameData(string playerName)
    {
        // player
        PlayerManager player = GameManager.Instance.SpawnPlayer();
        player.playerVariableManager.characterName.Value = playerName;
        player.playerVariableManager.health.MaxValue = 100;
        player.playerVariableManager.stamina.MaxValue = 4;
        player.playerVariableManager.currentArmorID.Value = 0;
        player.playerVariableManager.currentHelmetID.Value = 0;
        player.playerVariableManager.currentEquippedWeaponID.Value = 0;
        player.LoadGameDataFromCurrentCharacterDataFirst(ref currentGameData);
        // inventory
        WorldPlayerInventory.Instance.balance.Value = 10000;

        // gridworld
    }

    public void SaveGame()
    {
        _saveFileName = DecideCharacterFileNameBasedOnCharacterSlotBeingUsed(currentCharacterSlotBeingUsed);

        _saveFileDataWriter = new SaveFileDataWriter();
        _saveFileDataWriter.saveDataDirectoryPath = Application.persistentDataPath;
        _saveFileDataWriter.saveFileName = _saveFileName;

        GameManager.Instance.GetPlayer().SaveGameDataToCurrentCharacterData(ref currentGameData);

        _saveFileDataWriter.CreateNewCharacterSaveFile(currentGameData);
    }

    public void LoadGame()
    {
        // LOAD A PREVIOUS FILE, WITH A FILE NAME DEPENDING ON WHICH SLOT WE ARE USING
        _saveFileName = DecideCharacterFileNameBasedOnCharacterSlotBeingUsed(currentCharacterSlotBeingUsed);

        _saveFileDataWriter = new SaveFileDataWriter();
        _saveFileDataWriter.saveDataDirectoryPath = Application.persistentDataPath;
        _saveFileDataWriter.saveFileName = _saveFileName;
        
        currentGameData = _saveFileDataWriter.LoadSaveFile();
        
        //Debug.LogWarning("LOAD GAME");
        PlayerManager player = GameManager.Instance.SpawnPlayer();
        player.LoadGameDataFromCurrentCharacterDataFirst(ref currentGameData);
        

        StartCoroutine(LoadWorldScene());
    }

    public bool LoadLastGame()
    {
        currentCharacterSlotBeingUsed = FindMostRecentlyPlayedSlot();
        
        if (currentCharacterSlotBeingUsed == CharacterSlot.NoSlot)
        {
            return false;
        }
        
        _saveFileDataWriter = new SaveFileDataWriter();
        _saveFileDataWriter.saveDataDirectoryPath = Application.persistentDataPath;
        _saveFileDataWriter.saveFileName = DecideCharacterFileNameBasedOnCharacterSlotBeingUsed(currentCharacterSlotBeingUsed);
        
        if (_saveFileDataWriter.CheckToSeeIfFileExists())
        {
            LoadGame();
            return true;
        }
        else
        {
            return false;
        }
    }
    
    public void DeleteCurrentGame()
    {
        // CHOOSE FILE BASED ON NAME
        _saveFileDataWriter = new SaveFileDataWriter();
        _saveFileDataWriter.saveDataDirectoryPath = Application.persistentDataPath;
        _saveFileDataWriter.saveFileName = DecideCharacterFileNameBasedOnCharacterSlotBeingUsed(currentCharacterSlotBeingUsed);

        _saveFileDataWriter.DeleteSaveFile();
    }

    public void DeleteGame(CharacterSlot characterSlot)
    {
        // CHOOSE FILE BASED ON NAME
        _saveFileDataWriter = new SaveFileDataWriter();
        _saveFileDataWriter.saveDataDirectoryPath = Application.persistentDataPath;
        _saveFileDataWriter.saveFileName = DecideCharacterFileNameBasedOnCharacterSlotBeingUsed(characterSlot);

        _saveFileDataWriter.DeleteSaveFile();
    }
    
    public void DeleteAllGame()
    {
        foreach (CharacterSlot characterSlot in Enum.GetValues(typeof(CharacterSlot)))
        {
            _saveFileDataWriter.saveFileName = DecideCharacterFileNameBasedOnCharacterSlotBeingUsed(characterSlot);

            if (_saveFileDataWriter.CheckToSeeIfFileExists())
            {
                DeleteGame(characterSlot);
            }
        }
    }

    private void LoadAllCharacterProfiles()
    {
        _saveFileDataWriter = new SaveFileDataWriter();
        _saveFileDataWriter.saveDataDirectoryPath = Application.persistentDataPath;

        for (int i = 0; i < _maxCharacterSlots; i++)
        {
            CharacterSlot slot = (CharacterSlot)i;
            _saveFileDataWriter.saveFileName = DecideCharacterFileNameBasedOnCharacterSlotBeingUsed(slot);
            
            // 파일이 존재하는지 먼저 확인
            if (_saveFileDataWriter.CheckToSeeIfFileExists())
            {
                characterSlots[i] = _saveFileDataWriter.LoadSaveFile();
                //Debug.Log($"슬롯 {i} 로드 성공: {saveFileDataWriter.saveFileName}");
            }
            else
            {
                characterSlots[i] = null;
                //Debug.Log($"슬롯 {i} 파일 없음: {saveFileDataWriter.saveFileName}");
            }
        }
    }

    public SaveGameData GetSaveGameData(CharacterSlot slot)
    {
        int index = (int)slot;
        if (index >= 0 && index < characterSlots.Count)
        {
            return characterSlots[index];
        }
        return null;
    }

    public void SetSaveGameData(CharacterSlot slot, SaveGameData data)
    {
        int index = (int)slot;
        if (index >= 0 && index < characterSlots.Count)
        {
            characterSlots[index] = data;
        }
    }

    private IEnumerator LoadWorldScene()
    {
        WorldSceneChangeManager.Instance.LoadShelter();
        
        yield return null;
    }
    
    private CharacterSlot FindMostRecentlyPlayedSlot()
    {
        DateTime mostRecentTime = DateTime.MinValue;
        CharacterSlot mostRecentSlot = CharacterSlot.NoSlot; // 기본값 설정
        bool foundAnySave = false;
        
        _saveFileDataWriter = new SaveFileDataWriter();
        _saveFileDataWriter.saveDataDirectoryPath = Application.persistentDataPath;
        
        // 모든 슬롯을 확인
        for (int i = 0; i < characterSlots.Count; i++)
        {
            SaveGameData slotData = characterSlots[i];
            CharacterSlot currentSlot = (CharacterSlot)i;
            
            // 슬롯에 저장된 데이터가 있는지 확인
            if (slotData != null && !string.IsNullOrEmpty(slotData.lastPlayTime))
            {
                // 실제 파일이 존재하는지도 확인
                _saveFileDataWriter.saveFileName = DecideCharacterFileNameBasedOnCharacterSlotBeingUsed(currentSlot);
                if (_saveFileDataWriter.CheckToSeeIfFileExists())
                {
                    foundAnySave = true;
                    // ISO 8601 형식의 문자열을 DateTime으로 변환
                    DateTime slotDateTime = DateTime.Parse(slotData.lastPlayTime);
                
                    // 현재까지 발견한 가장 최근 시간보다 더 최근인지 확인
                    if (slotDateTime > mostRecentTime)
                    {
                        mostRecentTime = slotDateTime;
                        mostRecentSlot = currentSlot;
                    }
                    
                    //Debug.Log($"슬롯 {i} 확인됨: {slotData.characterName}, 시간: {slotDateTime}");
                }
                else
                {
                    //Debug.LogWarning($"슬롯 {i}에 데이터가 있지만 파일이 존재하지 않습니다: {saveFileDataWriter.saveFileName}");
                }
            }
        }
    
        // 저장된 게임을 찾았는지 여부를 로그로 출력
        if (foundAnySave)
        {
            //Debug.Log($"가장 최근 플레이: 슬롯 {(int)mostRecentSlot}, 시간: {mostRecentTime}");
            return mostRecentSlot;
        }
        else
        {
            //Debug.Log("저장된 게임이 없습니다.");
            return CharacterSlot.NoSlot; // 저장된 게임이 없으면 NO_SLOT 반환
        }
    }
}