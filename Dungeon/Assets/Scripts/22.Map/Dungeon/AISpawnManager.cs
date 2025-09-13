using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Random = UnityEngine.Random;

public class AISpawnManager : MonoBehaviour
{
    public static AISpawnManager Instance { get; private set; }
    private static AISpawnManager _instance;
    
    public SpawnAICharacterSO spawnableCharacters;
    
    [Header("Character")] 
    [SerializeField] private List<AICharacterSpawner> aiCharacterSpawners = new List<AICharacterSpawner>();
    private readonly List<AICharacterManager> _spawnedInCharacters = new List<AICharacterManager>();
    
    [Header("Spawn Settings")]
    [SerializeField] private int desiredEnemyCount = 20;

    private readonly Dictionary<int, int> _killLog = new Dictionary<int, int>();
    private Coroutine _enemyMaintenanceCoroutine;
    private bool _hasInitialSpawned = false;
    
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        ResetKillLog();
        StartCoroutine(InitialSpawnSequence());
    }
    
    private void OnDestroy()
    {
        // 오브젝트 파괴 시 코루틴 정리
        if (_enemyMaintenanceCoroutine != null)
        {
            StopCoroutine(_enemyMaintenanceCoroutine);
        }
    }
    
    // 게임 시작 시 초기 스폰 시퀀스
    private IEnumerator InitialSpawnSequence()
    {
        // 스포너들이 등록될 때까지 잠시 대기
        yield return new WaitForSeconds(0.5f);
        
        if (aiCharacterSpawners.Count == 0)
        {
            Debug.LogWarning("No spawners registered! Waiting for spawners...");
            yield return new WaitUntil(() => aiCharacterSpawners.Count > 0);
        }
        
        // 초기 적 생성
        yield return StartCoroutine(SpawnInitialEnemies());
        
        _hasInitialSpawned = true;
        
        // 유지보수 코루틴 시작
        _enemyMaintenanceCoroutine = StartCoroutine(MaintainEnemyCountCoroutine());
    }
    
    private IEnumerator SpawnInitialEnemies()
    {
        int spawnCount = 0;
        
        while (spawnCount < desiredEnemyCount && aiCharacterSpawners.Count > 0)
        {
            AICharacterSpawner randomSpawner = GetRandomSpawner();
            if (randomSpawner != null)
            {
                bool spawned = randomSpawner.AttemptToSpawnCharacter();
                if (spawned)
                {
                    spawnCount++;
                }
            }
            
            // 스폰 간격 조절 (너무 빠르게 스폰되지 않도록)
            yield return new WaitForSeconds(2f);
        }
        
        Debug.Log($"Initial spawn completed: {spawnCount}/{desiredEnemyCount} enemies spawned");
    }
    
    private AICharacterSpawner GetRandomSpawner()
    {
        if (aiCharacterSpawners.Count == 0)
            return null;
            
        // null이 아닌 스포너들만 필터링
        var activeSpawners = aiCharacterSpawners.Where(s => s != null).ToList();
        
        if (activeSpawners.Count == 0)
            return null;
            
        int randomIndex = UnityEngine.Random.Range(0, activeSpawners.Count);
        return activeSpawners[randomIndex];
    }
    
    public void RegisterSpawner(AICharacterSpawner aiCharacterSpawner)
    {
        aiCharacterSpawners.Add(aiCharacterSpawner);
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
    
    private IEnumerator MaintainEnemyCountCoroutine()
    {
        while (true)
        {
            // 초기 스폰이 완료된 후에만 유지보수 실행
            if (_hasInitialSpawned)
            {
                CheckAndMaintainEnemyCount();
            }
            yield return new WaitForSeconds(30f);
        }
    }

    private void CheckAndMaintainEnemyCount()
    {
        _spawnedInCharacters.RemoveAll(c => c == null || c.isDead.Value);

        int currentCount = _spawnedInCharacters.Count;
        int toSpawn = desiredEnemyCount - currentCount;

        if (toSpawn <= 0 || aiCharacterSpawners.Count == 0)
            return;
        
        AICharacterSpawner selectedSpawner = Random.Range(0f, 1f) < 0.5f ? GetClosestSpawnerToPlayer() : GetRandomSpawner();
        if (selectedSpawner != null)
        {
            selectedSpawner.AttemptToSpawnCharacter();
        }
    }
    
    public AICharacterSpawner GetClosestSpawnerToPlayer()
    {
        var player = GameManager.Instance.GetPlayer();
        if (player == null || aiCharacterSpawners.Count == 0)
            return null;

        Vector3 playerPosition = player.transform.position;
        AICharacterSpawner closestSpawner = null;
        float closestDistance = float.MaxValue;

        foreach (var spawner in aiCharacterSpawners)
        {
            if (spawner == null) continue;

            float distance = Vector3.Distance(playerPosition, spawner.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestSpawner = spawner;
            }
        }

        return closestSpawner;
    }
    
    public void NotifyTermination(AICharacterManager character)
    {
        // 킬로그 업데이트
        if (character != null)
        {
            int characterId = character.characterID;
            if (_killLog.ContainsKey(characterId))
            {
                _killLog[characterId]++;
            }
            else
            {
                _killLog[characterId] = 1;
            }
        }
    
        RemoveCharacterFromSpawnedList(character);
    }

    public int GetKillCount(int characterId)
    {
        return _killLog.GetValueOrDefault(characterId);
    }

    public int GetTotalKillCount()
    {
        return _killLog.Values.Sum();
    }

    public Dictionary<int, int> GetKillLog()
    {
        return new Dictionary<int, int>(_killLog);
    }

    public void ResetKillLog()
    {
        _killLog.Clear();
    }
    
    private void RemoveCharacterFromSpawnedList(AICharacterManager character)
    {
        _spawnedInCharacters.Remove(character);
    }
    
    // 디버그용 메서드들
    public int GetCurrentSpawnedCount()
    {
        return _spawnedInCharacters.Count(c => c != null && !c.isDead.Value);
    }
    
    public bool HasInitialSpawnCompleted()
    {
        return _hasInitialSpawned;
    }
}