using System;
using System.Collections;
using Unity.AI.Navigation;
using Unity.Mathematics;
using UnityEngine;

public class DungeonMapSetter : MonoBehaviour
{
    public int dungeonID;
    private NavMeshSurface _navMeshSurface;
    private MapGeneratorFactory _mapGenerator;
    
    [SerializeField] private GameObject playerStartPrefab;
    [SerializeField] private GameObject exitPrefab;
    Vector2Int playerSpawn, exit;
    
    [Header("NavMesh Build Settings")]
    [SerializeField] private bool useAsyncNavMeshBuild = true;
    [SerializeField] private float navMeshBuildDelay = 0.5f;
    
    public static event Action OnPlayerSpawned;
    public static event Action OnBossStart;
    public static event Action OnNavMeshBuilt;

    private void Awake()
    {
        _navMeshSurface = GetComponent<NavMeshSurface>();
        _mapGenerator = GetComponent<MapGeneratorFactory>();

        GameTimer.OnTimerEnd += SpawnBoss;
    }

    private void Start()
    {
        StartCoroutine(GenerateMapSequence());
    }

    private IEnumerator GenerateMapSequence()
    {
        // 1단계: 맵 생성
        GenerateMap();
        yield return new WaitForEndOfFrame();
        
        // 2단계: NavMesh 비동기 빌드
        if (useAsyncNavMeshBuild)
        {
            yield return StartCoroutine(BuildNavMeshAsync());
        }
        else
        {
            yield return new WaitForSeconds(navMeshBuildDelay);
            _navMeshSurface.BuildNavMesh();
        }
        
        // 3단계: 플레이어 스폰
        Vector3 offset = _mapGenerator.GetCubeSize();
        GeneratePlayerSpawn(offset);
    }

    private IEnumerator BuildNavMeshAsync()
    {
        // NavMesh 빌드 전 잠시 대기하여 다른 시스템들이 안정화되도록 함
        yield return new WaitForSeconds(navMeshBuildDelay);
        
        // 점진적 NavMesh 빌드
        _navMeshSurface.BuildNavMesh();
        
        Debug.Log("NavMesh 빌드 완료");
        OnNavMeshBuilt?.Invoke();
    }

    private void GenerateMap()
    {
        _mapGenerator.GenerateMap();
    }

    private void GeneratePlayerSpawn(Vector3 offset)
    {
        playerSpawn = _mapGenerator.CurrentGenerator.GetStartPos();
        exit = _mapGenerator.CurrentGenerator.GetExitPos();

        Instantiate(playerStartPrefab, new Vector3(playerSpawn.x * offset.x, 0f, playerSpawn.y * offset.z), quaternion.identity);
        Instantiate(exitPrefab, new Vector3(exit.x * offset.x, 0f, exit.y * offset.z), quaternion.identity);
        
        ActivateGameTimerWithEvent();
    }
    
    private void ActivateGameTimerWithEvent()
    {
        // 플레이어 스폰 이벤트 발생
        OnPlayerSpawned?.Invoke();
        Debug.Log("Player Spawn");
    }

    private void SpawnBoss()
    {
        var spawner = AISpawnManager.Instance.GetClosestSpawnerToPlayer();
        spawner.AttemptToSpawnCharacter(true);
        Debug.Log("Time Over Boss Spawns");
        OnBossStart?.Invoke();
    }
}