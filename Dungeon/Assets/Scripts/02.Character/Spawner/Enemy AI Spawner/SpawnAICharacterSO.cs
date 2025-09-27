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
}
