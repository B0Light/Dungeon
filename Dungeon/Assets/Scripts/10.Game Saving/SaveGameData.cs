using System;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
// SINCE WE WANT TO REFERENCE THIS DATA FOR EVERY SAVE FILE, THIS SCRIPT IS NOT A MONOBEHAVIOR AND IS INSTEAD SERIALIZABLE
public class SaveGameData
{
    [Header("Scene Index")]
    public int sceneIndex = 3;
    
    [Header("Character Name")]
    public string characterName = "Gangdodan";

    [Header("Time Played")]
    public int secondsPlayed;
    public string lastPlayTime; // ISO 8601 형식으로 시간 저장

    [Header("Status")]
    public float curHealthPercent;

    [Header("Money")] public int balance;

    [Header("Inventory Size")] 
    public Vector2Int weaponBoxSize;
    public Vector2Int helmetBoxSize;
    public Vector2Int armorBoxSize;
    public Vector2Int consumableBoxSize;
    
    public Vector2Int inventoryBoxSize;
    public Vector2Int backpackSize;
    public Vector2Int shareBoxSize;
    public Vector2Int safeBoxSize;
    
    [Header("Inventory")] 
    public int weaponItemCode;
    public int helmetItemCode;
    public int armorItemCode;
    // key : itemID / value : itemCount
    public SerializableDictionary<int, int> quickSlotConsumableItems;
    public SerializableDictionary<int, int> inventoryItems;
    public SerializableDictionary<int, int> backpackItems;
    public SerializableDictionary<int, int> safeItems;
    [Header("ShareInventory")] 
    public SerializableDictionary<int, int> shareInventoryItems;

    [Header("Perk")] 
    public SerializableBitSet unlockPerkList;
    // 1xx : 카테고리 (손, 머리...) / 2.xx : 티어 / 3.xx 해당 인덱스 (0: 메인 1. 서브 1...)
    
    [SerializeReference]
    public List<SaveBuildingData> buildings; // 현재 Shelter에 건설된 건물 정보를 저장 

    [Header("Dungeon")] 
    public SerializableDictionary<int, bool> availableDungeon;

    [Header("Shelter")] 
    public int shelterLevel;
    public bool isVisitedToday;
    
    // default Value 
    public SaveGameData()
    {
        lastPlayTime = DateTime.Now.ToString("o");
        weaponBoxSize = new Vector2Int(4, 1);
        helmetBoxSize = new Vector2Int(2, 2);
        armorBoxSize = new Vector2Int(2, 2);
        consumableBoxSize = new Vector2Int(4, 1);

        backpackSize = new Vector2Int(0,0);
        inventoryBoxSize = new Vector2Int(6, 3);
        safeBoxSize = new Vector2Int(2, 2);
        shareBoxSize = new Vector2Int(8, 20);

        curHealthPercent = 1f;
        
        inventoryItems = new SerializableDictionary<int, int>();
        backpackItems = new SerializableDictionary<int, int>();
        safeItems = new SerializableDictionary<int, int>();
        quickSlotConsumableItems = new SerializableDictionary<int, int>();
        shareInventoryItems = new SerializableDictionary<int, int>();

        unlockPerkList = new SerializableBitSet(1000);
        unlockPerkList.Set(0, true);
        
        // Default Building
        buildings = new List<SaveBuildingData>();
        
        availableDungeon = new SerializableDictionary<int, bool>();
        availableDungeon.TryAdd(0, true);
        availableDungeon.TryAdd(1, false);
        availableDungeon.TryAdd(2, false);
        availableDungeon.TryAdd(3, false);
        availableDungeon.TryAdd(4, false);
        availableDungeon.TryAdd(5, false);
        availableDungeon.TryAdd(6, false);
        availableDungeon.TryAdd(7, false);
        availableDungeon.TryAdd(8, false);
        availableDungeon.TryAdd(9, false);
        
        shelterLevel = 0;
        balance = 10000;
        secondsPlayed = 0;
        isVisitedToday = false;
    }
    
    // JSON 저장 전 데이터 변환
    public void PrepareForSerialization()
    {
        unlockPerkList.PrepareForSerialization();
    }

    // JSON 로드 후 데이터 복원
    public void RestoreFromSerialization()
    {
        unlockPerkList.RestoreFromSerialization(1000);
    }
}

