using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class WorldCharacterDropItem : Singleton<WorldCharacterDropItem>
{
    public SerializableDictionary<int, List<DropItem>> dropItemDic = new SerializableDictionary<int, List<DropItem>>();

    private void Start()
    {
        LoadDropTable("Assets/Data/Sheet_DropItem/DropItem.csv");
    }

    private void LoadDropTable(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                Debug.LogError($"CSV file not found: {filePath}");
                return;
            }

            string[] lines = File.ReadAllLines(filePath);
            
            if (lines.Length <= 1)
            {
                Debug.LogWarning("CSV file is empty or has no data rows");
                return;
            }

            // 헤더 확인 (디버그용)
            Debug.Log($"CSV Header: {lines[0]}");
            
            for (int i = 1; i < lines.Length; i++)
            {
                // 빈 줄 건너뛰기
                if (string.IsNullOrWhiteSpace(lines[i])) continue;
                
                string[] data = lines[i].Split(',');
                
                // 데이터 유효성 검사 - 기본 3개 컬럼 필요
                if (data.Length < 3) 
                {
                    Debug.LogWarning($"Invalid data format at line {i + 1}: {lines[i]}");
                    continue;
                }
                
                // 데이터 파싱
                if (int.TryParse(data[0].Trim(), out int monsterID) && 
                    int.TryParse(data[1].Trim(), out int itemID) && 
                    float.TryParse(data[2].Trim(), out float dropRate))
                {
                    // 추가 컬럼들 파싱 (있는 경우만)
                    int minCount = 1;
                    int maxCount = 1;
                    bool isGuaranteed = false;
                    
                    if (data.Length >= 4 && int.TryParse(data[3].Trim(), out int parsedMinCount))
                        minCount = parsedMinCount;
                    
                    if (data.Length >= 5 && int.TryParse(data[4].Trim(), out int parsedMaxCount))
                        maxCount = parsedMaxCount;
                    
                    if (data.Length >= 6 && bool.TryParse(data[5].Trim(), out bool parsedIsGuaranteed))
                        isGuaranteed = parsedIsGuaranteed;

                    DropItem dropItem = new DropItem(itemID, dropRate, minCount, maxCount, isGuaranteed);

                    if (!dropItemDic.ContainsKey(monsterID))
                    {
                        dropItemDic[monsterID] = new List<DropItem>();
                    }

                    dropItemDic[monsterID].Add(dropItem);
                }
                else
                {
                    Debug.LogWarning($"Invalid data at line {i + 1}: {lines[i]}");
                }
            }
            
            Debug.Log($"Successfully loaded {dropItemDic.Count} monster drop tables");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load drop table: {e.Message}");
        }
    }
    
    /// <summary>
    /// 몬스터가 드롭하는 아이템 목록을 계산하여 반환
    /// </summary>
    public List<int> GetDroppedItems(int monsterID)
    {
        List<int> droppedItems = new List<int>();
        
        if (!dropItemDic.ContainsKey(monsterID))
        {
            Debug.LogWarning($"No drop table found for monster ID: {monsterID}");
            return droppedItems;
        }
        
        foreach (var dropItem in dropItemDic[monsterID])
        {
            // 확정 드롭이거나 확률에 성공한 경우
            if (dropItem.isGuaranteed || Random.Range(0f, 1f) <= dropItem.dropRate)
            {
                int dropCount = dropItem.GetDropCount();
                for (int i = 0; i < dropCount; i++)
                {
                    droppedItems.Add(dropItem.itemID);
                }
            }
        }
        
        return droppedItems;
    }
    
    /// <summary>
    /// 특정 아이템이 해당 몬스터에서 드롭되는지 확인
    /// </summary>
    public bool CanDropItem(int monsterID, int itemID)
    {
        if (!dropItemDic.ContainsKey(monsterID)) return false;
        
        return dropItemDic[monsterID].Exists(item => item.itemID == itemID);
    }
    
    /// <summary>
    /// 몬스터의 모든 드롭 아이템 정보 가져오기
    /// </summary>
    public List<DropItem> GetMonsterDropItems(int monsterID)
    {
        return dropItemDic.ContainsKey(monsterID) ? dropItemDic[monsterID] : new List<DropItem>();
    }
    
    /// <summary>
    /// 드롭률 동적 조정 (이벤트 등에서 사용)
    /// </summary>
    public void ModifyDropRate(int monsterID, int itemID, float multiplier)
    {
        if (!dropItemDic.ContainsKey(monsterID)) return;
        
        var item = dropItemDic[monsterID].Find(x => x.itemID == itemID);
        if (item != null)
        {
            item.dropRate = Mathf.Clamp01(item.dropRate * multiplier);
            Debug.Log($"Modified drop rate for Monster {monsterID}, Item {itemID}: {item.dropRate}");
        }
    }
    
    /// <summary>
    /// 특정 몬스터의 드롭 정보를 디버그 로그로 출력
    /// </summary>
    public void DebugMonsterDrops(int monsterID)
    {
        if (!dropItemDic.ContainsKey(monsterID))
        {
            Debug.Log($"No drops found for Monster ID: {monsterID}");
            return;
        }
        
        Debug.Log($"=== Monster {monsterID} Drops ===");
        foreach (var drop in dropItemDic[monsterID])
        {
            Debug.Log($"Item {drop.itemID}: {drop.dropRate * 100}% chance, {drop.minCount}-{drop.maxCount} count{(drop.isGuaranteed ? " (Guaranteed)" : "")}");
        }
    }
    
    /// <summary>
    /// 전체 드롭 테이블 통계 출력
    /// </summary>
    public void PrintDropTableStats()
    {
        int totalMonsters = dropItemDic.Count;
        int totalDropItems = 0;
        
        foreach (var kvp in dropItemDic)
        {
            totalDropItems += kvp.Value.Count;
        }
        
        Debug.Log($"Drop Table Stats - Monsters: {totalMonsters}, Total Drop Items: {totalDropItems}");
    }
}

// 드롭 아이템 정보를 저장할 클래스 정의
[System.Serializable]
public class DropItem
{
    public int itemID;
    public float dropRate;
    public int minCount = 1;
    public int maxCount = 1;
    public bool isGuaranteed = false; // 확정 드롭 여부

    public DropItem(int itemID, float dropRate, int minCount = 1, int maxCount = 1, bool isGuaranteed = false)
    {
        this.itemID = itemID;
        this.dropRate = Mathf.Clamp01(dropRate); // 0~1 범위로 제한
        this.minCount = Mathf.Max(1, minCount); // 최소 1개
        this.maxCount = Mathf.Max(minCount, maxCount); // minCount보다 작을 수 없음
        this.isGuaranteed = isGuaranteed;
    }
    
    /// <summary>
    /// 실제 드롭될 개수 계산
    /// </summary>
    public int GetDropCount()
    {
        return Random.Range(minCount, maxCount + 1);
    }
    
    /// <summary>
    /// 드롭 정보를 문자열로 반환
    /// </summary>
    public override string ToString()
    {
        return $"ItemID: {itemID}, DropRate: {dropRate:P}, Count: {minCount}-{maxCount}{(isGuaranteed ? " (Guaranteed)" : "")}";
    }
}