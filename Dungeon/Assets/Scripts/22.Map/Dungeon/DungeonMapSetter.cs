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
    
    [SerializeField] private Transform floorSlot;
    [SerializeField] private Transform roomSlot;
    private NavMeshSurface _navMeshSurface;
    private MapGenerator _mapGenerator;
    
    //==========================================================
    public Transform mapParent;
    private MapGridPathfinder _pathfinder;
    private List<GameObject> _units = new List<GameObject>();
    
    [Header("Pathfinding")]
    public GameObject playerPrefab;
    public GameObject enemyPrefab;
    //==========================================================
    
    [SerializeField] private GameObject playerStartPrefab;
    [SerializeField] private GameObject exitPrefab;
    Vector2Int playerSpawn, exit;
    
    [Header("NavMesh Build Settings")]
    [SerializeField] private bool useAsyncNavMeshBuild = true;
    [SerializeField] private float navMeshBuildDelay = 0.5f;

    private void Awake()
    {
        _navMeshSurface = GetComponent<NavMeshSurface>();
        _mapGenerator = GetComponent<MapGenerator>();
    }

    private void Start()
    {
        _mapGenerator.InitGenerator(floorSlot, dungeonDataSo);
        StartCoroutine(GenerateMapSequence());
    }

    private IEnumerator GenerateMapSequence()
    {
        // 1단계 : 맵 생성
        GenerateMap();
        yield return new WaitForEndOfFrame();
        // 2단계 : 방 생성 
        GenerateRoom();
        // 3단계 : NavMesh 비동기 빌드
        if (useAsyncNavMeshBuild)
        {
            yield return StartCoroutine(BuildNavMeshAsync());
        }
        else
        {
            yield return new WaitForSeconds(navMeshBuildDelay);
            _navMeshSurface.BuildNavMesh();
        }
        //==========================================================
        // 4단계 : PathFinder 생성
        var mapData = _mapGenerator.GetMapData();
        _pathfinder = new MapGridPathfinder(mapData);
        
        
        _pathfinder.AllowDiagonalMovement = false; // 4방향 이동만
        _pathfinder.StraightMoveCost = 1.0f;
        
        Debug.Log("맵과 패스파인딩 시스템이 초기화되었습니다!");
        
        SpawnUnits();
        //============================================================
    }

    private IEnumerator BuildNavMeshAsync()
    {
        // NavMesh 빌드 전 잠시 대기하여 다른 시스템들이 안정화되도록 함
        yield return new WaitForSeconds(navMeshBuildDelay);
        
        // 점진적 NavMesh 빌드
        _navMeshSurface.BuildNavMesh();
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
            GameObject targetBuilding = null;

            if (buildingQueue.Count > 0)
            {
                targetBuilding = buildingQueue.Dequeue();
            }
            else if (subBuildingList.Count > 0)
            {
                targetBuilding = subBuildingList[UnityEngine.Random.Range(0, subBuildingList.Count)];
            }
        
            if (targetBuilding != null)
            {
                InstantiateBuilding(targetBuilding, room, mapData);
            }
        }
    }

    private void InstantiateBuilding(GameObject targetBuilding, RectInt room, MapData mapData)
    {
        Vector3 position = new Vector3(
            room.center.x * mapData.mapConfig.CubeSize.x,
            mapData.mapConfig.CubeSize.y,
            room.center.y * mapData.mapConfig.CubeSize.z
        );

        Quaternion rotation = GetBuildingRotation(room);

        GameObject instantiatedRoom = Instantiate(targetBuilding, position, rotation, roomSlot);
        instantiatedRoom.transform.localScale = mapData.mapConfig.CubeSize;
    }

    private Quaternion GetBuildingRotation(RectInt room)
    {
        if (_mapGenerator.GetRoomDirection().TryGetValue(room, out var gateDirections))
        {
            // Define a set of all possible directions.
            List<Vector2Int> allDirections = new List<Vector2Int>
            {
                Vector2Int.up,
                Vector2Int.down,
                Vector2Int.left,
                Vector2Int.right
            };
        
            foreach (var direction in allDirections)
            {
                if (!gateDirections.Contains(direction))
                {
                    if (direction == Vector2Int.right) return Quaternion.Euler(0, 90, 0); // To face Right
                    if (direction == Vector2Int.up) return Quaternion.Euler(0, 0, 0);   // To face Up
                    if (direction == Vector2Int.left) return Quaternion.Euler(0, 270, 0); // To face Left
                    if (direction == Vector2Int.down) return Quaternion.Euler(0, 180, 0); // To face Down
                }
            }
        
            if (gateDirections.Any())
            {
                Vector2Int firstGateDirection = gateDirections[0];
                if (firstGateDirection == Vector2Int.right) return Quaternion.Euler(0, 270, 0);
                if (firstGateDirection == Vector2Int.up) return Quaternion.Euler(0, 180, 0);
                if (firstGateDirection == Vector2Int.left) return Quaternion.Euler(0, 90, 0);
                if (firstGateDirection == Vector2Int.down) return Quaternion.Euler(0, 0, 0);
            }
        }
    
        return Quaternion.identity;
    }
    
    
    /* Path Finding */
    // 이하 임시 코드 
    private void SpawnUnits()
    {
        // 맵에서 이동 가능한 위치 찾기
        var walkablePositions = FindWalkablePositions();
        
        if (walkablePositions.Count < 2)
        {
            Debug.LogError("이동 가능한 위치가 부족합니다!");
            return;
        }
        
        // 플레이어 생성
        SpawnUnit(playerPrefab, walkablePositions[0], "Player");
        
        // 적 생성
        for (int i = 1; i < Mathf.Min(5, walkablePositions.Count); i++)
        {
            SpawnUnit(enemyPrefab, walkablePositions[i], $"Enemy_{i}");
        }
    }
    
    private void SpawnUnit(GameObject prefab, Vector2Int gridPosition, string unitName)
    {
        GameObject unit = Instantiate(prefab, mapParent);
        unit.name = unitName;
        
        // GridMovementController 컴포넌트 추가 및 초기화
        var controller = unit.GetComponent<GridMovementController>();
        if (controller == null)
        {
            controller = unit.AddComponent<GridMovementController>();
        }
        
        // 맵에 맞는 셀 크기 설정 (dungeonData에서 가져오기)
        controller.cellSize = new Vector3(_mapGenerator.GetMapData().mapConfig.CubeSize.x, _mapGenerator.GetMapData().mapConfig.CubeSize.y, _mapGenerator.GetMapData().mapConfig.CubeSize.z);
        controller.gridOffset = Vector3.zero;
        controller.moveSpeed = 3f;
        controller.allowDiagonalMovement = false;
        
        // 패스파인더와 시작 위치로 초기화
        controller.Initialize(_pathfinder, gridPosition);
        
        _units.Add(unit);
        
        Debug.Log($"{unitName} 생성됨 at {gridPosition}");
        
        InvokeRepeating(nameof(MoveRandomly), 2f, 2f);
    }
    
    private List<Vector2Int> FindWalkablePositions()
    {
        var walkablePositions = new List<Vector2Int>();
        var mapData = _mapGenerator.GetMapData();
        var gridData = mapData.mapConfig;
        
        for (int x = 0; x < gridData.GridSize.x; x++)
        {
            for (int y = 0; y < gridData.GridSize.y; y++)
            {
                var pos = new Vector2Int(x, y);
                if (_pathfinder.IsPositionWalkable(pos))
                {
                    walkablePositions.Add(pos);
                }
            }
        }
        
        return walkablePositions;
    }
    
    private void MoveRandomly()
    {
        foreach (var unit in _units)
        {
            var controller = unit.GetComponent<GridMovementController>();
            var randomPos = GetRandomWalkablePosition();
            controller.MoveTo(randomPos);
        }
    }

    private Vector2Int GetRandomWalkablePosition()
    {
        var grid = FindWalkablePositions();
        if (grid == null || grid.Count == 0)
        {
            return Vector2Int.zero; 
        }
        int randomIndex = UnityEngine.Random.Range(0, grid.Count);
        return grid[randomIndex];
    }
    
}