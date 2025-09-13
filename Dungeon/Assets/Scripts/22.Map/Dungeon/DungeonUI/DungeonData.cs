using System.Collections.Generic;
using System.Text;
using UnityEngine;

[CreateAssetMenu(menuName = "Dungeon/Info ")]
public class DungeonData : ScriptableObject
{
    [Header("Dungeon Information")] 
    public int dungeonID;
    public int dungeonKey;
    public Difficulty difficulty;
    public string dungeonSceneName;
    public string dungeonName;

    public List<int> enemyList;
    public List<int> mainResourceList;
    public int bossSpawnTimer ;

    public Sprite dungeonInfoBackground;
    public AudioSource bgm;
    
    public string GetFormattedInfo()
    {
        StringBuilder info = new StringBuilder();
    
        info.AppendLine($"[난이도 : {difficulty}]");
        info.AppendLine();
        info.AppendLine($"출현 몬스터 : {GetEnemyNames()}");
        info.AppendLine();
        info.AppendLine($"핵심 자원 : {GetResourceNames()}");
        info.AppendLine();
        info.AppendLine($"던전 폐쇄 시간 : {bossSpawnTimer}분");
        info.AppendLine();
    
        return info.ToString();
    }

    private string GetEnemyNames()
    {
        StringBuilder info = new StringBuilder();
        List<string> enemyNames = new List<string>();
        foreach (int enemyID in enemyList)
        {
            string enemyName = WorldDatabase_Enemy.Instance.GetNameById(enemyID);
            if (!string.IsNullOrEmpty(enemyName))
                enemyNames.Add(enemyName);
        }
        info.AppendLine(string.Join(", ", enemyNames));
        return info.ToString();
    }

    private string GetResourceNames()
    {
        StringBuilder info = new StringBuilder();
        List<string> resourceName = new List<string>();
        foreach (int itemID in mainResourceList)
        {
            var itemInfo = WorldDatabase_Item.Instance.GetItemByID(itemID);
            if (!string.IsNullOrEmpty(itemInfo.itemName))
                resourceName.Add(itemInfo.itemName);
        }
        info.AppendLine(string.Join(", ", resourceName));
        return info.ToString();
    }
    
}
