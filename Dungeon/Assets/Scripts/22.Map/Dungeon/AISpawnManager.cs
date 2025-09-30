using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AISpawnManager : MonoBehaviour
{
    public static AISpawnManager Instance { get; private set; }
    
    private readonly List<AICharacterSpawner_Grid> _aiCharacterSpawners = new List<AICharacterSpawner_Grid>();
    private readonly List<AICharacterManager> _spawnedInCharacters = new List<AICharacterManager>();

    private GridPathfinder _pathfinder;
    private void Awake()
    {
        Instance = this;
    }

    public void Init(GridPathfinder pathfinder)
    {
        _pathfinder = pathfinder;

        StartCoroutine(InitialSpawnSequence());
    }
    
    
    // 게임 시작 시 초기 스폰 시퀀스
    private IEnumerator InitialSpawnSequence()
    {
        // 스포너들이 등록될 때까지 잠시 대기
        yield return new WaitForSeconds(0.5f);
        
        if (_aiCharacterSpawners.Count == 0)
        {
            Debug.LogWarning("No spawners registered! Waiting for spawners...");
            yield return new WaitUntil(() => _aiCharacterSpawners.Count > 0);
        }
        
        yield return StartCoroutine(SpawnEnemies());
    }
    
    private IEnumerator SpawnEnemies()
    {
        foreach (var spawner in _aiCharacterSpawners)
        {
            yield return new WaitForEndOfFrame();
            spawner.SpawnUnit(_pathfinder);
        }
    }
    
    
    public void RegisterSpawner(AICharacterSpawner_Grid aiCharacterSpawnerGrid)
    {
        _aiCharacterSpawners.Add(aiCharacterSpawnerGrid);
        aiCharacterSpawnerGrid.Init();
    }

    public void AddCharacterToSpawnedCharactersList(AICharacterManager character)
    {
        if(_spawnedInCharacters.Contains(character))
            return;
        
        _spawnedInCharacters.Add(character);
    }
    
    private void DespawnAllCharacters()
    {
        foreach (var character in _spawnedInCharacters)
        {
            Destroy(character.gameObject);
        }
        _spawnedInCharacters.Clear();
    }
    
    
}