using System;
using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using System.Linq;

public class DungeonMapSetter : MonoBehaviour
{
    public int dungeonID;
    [SerializeField] private DungeonDataSO dungeonDataSo;
    [SerializeField] private DungeonRoomDataSO dungeonRoomDataSo;
    [SerializeField] private Transform roomSlot;
    private NavMeshSurface _navMeshSurface;
    private MapGenerator _mapGenerator;
    
    [SerializeField] private GameObject playerStartPrefab;
    [SerializeField] private GameObject exitPrefab;
    Vector2Int playerSpawn, exit;
    
    [Header("NavMesh Build Settings")]
    [SerializeField] private bool useAsyncNavMeshBuild = true;
    [SerializeField] private float navMeshBuildDelay = 0.5f;
    
    public static event Action OnPlayerSpawned;
    public static event Action OnNavMeshBuilt;

    private void Awake()
    {
        _navMeshSurface = GetComponent<NavMeshSurface>();
        _mapGenerator = GetComponent<MapGenerator>();
    }

    private void Start()
    {
        _mapGenerator.InitGenerator(dungeonDataSo);
        StartCoroutine(GenerateMapSequence());
    }

    private IEnumerator GenerateMapSequence()
    {
        // 1단계: 맵 생성
        GenerateMap();
        yield return new WaitForEndOfFrame();
        GenerateRoom();
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
        ActivateGameTimerWithEvent();
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

    private void GenerateRoom()
    {
        MapData mapData = _mapGenerator.GetMapData();
        Queue<GameObject> buildingQueue = new Queue<GameObject>(dungeonRoomDataSo.essentialBuilding);
        List<GameObject> subBuildingList = new List<GameObject>(dungeonRoomDataSo.subBuilding);
        foreach (var room in mapData.floorList)
        {
            if (buildingQueue.Count > 0)
            {
                var targetBuilding = buildingQueue.Dequeue();
                Vector3 position = new Vector3(room.center.x * mapData.mapConfig.CubeSize.x, mapData.mapConfig.CubeSize.y, room.center.y * mapData.mapConfig.CubeSize.z); 
                
                Quaternion rotation = Quaternion.identity;
                if (_mapGenerator.GetRoomDirection().TryGetValue(room, out var gateDirections))
                {
                    if (gateDirections.Any())
                    {
                        Vector2Int firstGateDirection = gateDirections[0];
                        // Vector2Int 방향을 Quaternion으로 변환
                        if (firstGateDirection == Vector2Int.right) rotation = Quaternion.Euler(0, 90, 0);
                        else if (firstGateDirection == Vector2Int.up) rotation = Quaternion.Euler(0, 180, 0);
                        else if (firstGateDirection == Vector2Int.left) rotation = Quaternion.Euler(0, 270, 0);
                        else if (firstGateDirection == Vector2Int.down) rotation = Quaternion.Euler(0, 0, 0);
                    }
                }
                GameObject instantiateRoom = Instantiate(targetBuilding, position, rotation, roomSlot);
                instantiateRoom.transform.localScale = mapData.mapConfig.CubeSize;
            }
            else if(subBuildingList.Count > 0)
            {
                var targetBuilding = subBuildingList[UnityEngine.Random.Range(0,subBuildingList.Count)];
                Vector3 position = new Vector3(room.center.x * mapData.mapConfig.CubeSize.x, mapData.mapConfig.CubeSize.y, room.center.y * mapData.mapConfig.CubeSize.z); 
                
                Quaternion rotation = Quaternion.identity;
                if (_mapGenerator.GetRoomDirection().TryGetValue(room, out var gateDirections))
                {
                    if (gateDirections.Any())
                    {
                        Vector2Int firstGateDirection = gateDirections[0];
                        // Vector2Int 방향을 Quaternion으로 변환
                        if (firstGateDirection == Vector2Int.right) rotation = Quaternion.Euler(0, 90, 0);
                        else if (firstGateDirection == Vector2Int.up) rotation = Quaternion.Euler(0, 180, 0);
                        else if (firstGateDirection == Vector2Int.left) rotation = Quaternion.Euler(0, 270, 0);
                        else if (firstGateDirection == Vector2Int.down) rotation = Quaternion.Euler(0, 0, 0);
                    }
                }
                GameObject instantiateRoom = Instantiate(targetBuilding, position, rotation, roomSlot);
                instantiateRoom.transform.localScale = mapData.mapConfig.CubeSize;
            }
        }
    }
    
    private void ActivateGameTimerWithEvent()
    {
        // 플레이어 스폰 이벤트 발생
        OnPlayerSpawned?.Invoke();
        Debug.Log("Player Spawn");
    }

    
}