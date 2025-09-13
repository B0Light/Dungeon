using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AI LIST", menuName = "Dungeon/AI Prefab")]
public class SpawnAICharacterSO : ScriptableObject
{
    public List<SpawnableCharacter> spawnData = new List<SpawnableCharacter>();
    public SpawnableCharacter stageBossData;
}

[System.Serializable]
public class SpawnableCharacter
{
    [Header("Character Info")]
    public GameObject characterPrefab;
    public int maxHealth = 100;
    
    [Header("Spawn Settings")]
    [Range(0, 10)]
    public int spawnWeight = 1; // 스폰 가중치 (높을수록 자주 스폰)
}
