using UnityEngine;
using System.Collections.Generic;

public class WorldDatabase_Enemy : Singleton<WorldDatabase_Enemy>
{
    [SerializeField] private GameObject enemyInfoPrefab;
    [SerializeField] private List<Sprite> itemIcons = new List<Sprite>();
    
    private Dictionary<int, Sprite> iconDictionary;
    private Dictionary<int, string> iconNameDictionary;

    void Start()
    {
        LoadItemIcons();
    }

    void LoadItemIcons()
    {
        iconDictionary = new Dictionary<int, Sprite>();
        iconNameDictionary = new Dictionary<int, string>();

        foreach (Sprite icon in itemIcons)
        {
            string[] parts = icon.name.Split('.');
            if (parts.Length >= 2 && int.TryParse(parts[0], out int key))
            {
                // 아이콘 딕셔너리 등록
                if (!iconDictionary.TryAdd(key, icon))
                {
                    Debug.LogWarning($"Duplicate sprite key: {key} for sprite {icon.name}");
                }

                // 이름 딕셔너리 등록
                if (!iconNameDictionary.TryAdd(key, parts[1]))
                {
                    Debug.LogWarning($"Duplicate name key: {key} for name {parts[1]}");
                }
            }
            else
            {
                Debug.LogWarning($"Invalid sprite name format: {icon.name}");
            }
        }

        Debug.Log($"Loaded {iconDictionary.Count} icons and {iconNameDictionary.Count} names.");
    }

    public Sprite GetIconById(int id)
    {
        if (iconDictionary.TryGetValue(id, out Sprite icon))
        {
            return icon;
        }

        Debug.LogWarning($"Icon not found for ID: {id}");
        return null;
    }

    public string GetNameById(int id)
    {
        if (iconNameDictionary.TryGetValue(id, out string name))
        {
            return name;
        }

        Debug.LogWarning($"Name not found for ID: {id}");
        return null;
    }

    public GameObject CreateEnemyInfo(int id, int killCnt, Transform parent)
    {
        GameObject enemyInfoInst = Instantiate(enemyInfoPrefab, parent);
        EnemyInfoHUD enemyInfoHUD = enemyInfoInst.GetComponent<EnemyInfoHUD>();
        enemyInfoHUD.Init(id, killCnt);

        return enemyInfoInst;
    }
}