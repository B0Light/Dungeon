using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AISpawnManager : MonoBehaviour
{
    public static AISpawnManager Instance { get; private set; }
    
    private readonly List<AICharacterSpawner_AStar> _aiCharacterSpawners_AStar = new List<AICharacterSpawner_AStar>();
    private readonly List<AICharacterSpawner_Navmesh> _aiCharacterSpawners_Navmesh = new List<AICharacterSpawner_Navmesh>();
    private readonly List<AICharacterManager> _spawnedInCharacters = new List<AICharacterManager>();
    private readonly List<Vector2Int> _patrolPointList = new List<Vector2Int>();
    
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
        
        if (_aiCharacterSpawners_AStar.Count == 0 &&  _aiCharacterSpawners_Navmesh.Count == 0)
        {
            Debug.LogWarning("No spawners registered! Waiting for spawners...");
            yield return new WaitUntil(() => _aiCharacterSpawners_AStar.Count + _aiCharacterSpawners_Navmesh.Count > 0);
        }
        
        yield return StartCoroutine(SpawnEnemies());
    }
    
    private IEnumerator SpawnEnemies()
    {
        foreach (var spawner in _aiCharacterSpawners_AStar)
        {
            yield return new WaitForEndOfFrame();
            spawner.SpawnUnit(_pathfinder, _patrolPointList);
        }
        foreach (var spawner in _aiCharacterSpawners_Navmesh)
        {
            yield return new WaitForEndOfFrame();
            spawner.SpawnUnit(_patrolPointList);
        }
    }
    
    
    public void RegisterSpawner(AICharacterSpawner_AStar aiCharacterSpawnerAStar)
    {
        _aiCharacterSpawners_AStar.Add(aiCharacterSpawnerAStar);
        _patrolPointList.Add(aiCharacterSpawnerAStar.Init());
    }
    
    public void RegisterSpawner(AICharacterSpawner_Navmesh aiCharacterSpawnerNavmesh)
    {
        _aiCharacterSpawners_Navmesh.Add(aiCharacterSpawnerNavmesh);
        _patrolPointList.Add(aiCharacterSpawnerNavmesh.Init());
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