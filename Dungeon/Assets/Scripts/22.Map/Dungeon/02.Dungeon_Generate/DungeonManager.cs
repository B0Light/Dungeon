using System;
using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using System.Linq;
using UnityEngine.Serialization;

public class DungeonManager : MonoBehaviour
{
    public int dungeonID;
    [FormerlySerializedAs("dungeonDataSo")] [SerializeField] private DungeonGenerateDataSO dungeonGenerateDataSo;
    [SerializeField] private DungeonRoomListDataSO dungeonRoomListDataSo;
    
    [SerializeField] private Transform floorSlot;
    [SerializeField] private Transform roomSlot;
    
    private NavMeshSurface _navMeshSurface;
    private MapGenerator _mapGenerator;
    private GridPathfinder _pathfinder;
    private AISpawnManager _aiSpawnManager;

    public FixedGridXZ<GridCell> FixedGrid { get; private set; }

    [Header("NavMesh Build Settings")]
    [SerializeField] private bool useAsyncNavMeshBuild = true;
    [SerializeField] private float navMeshBuildDelay = 0.5f;
    
    public event Action OnGeneratedDungeon;

    private void Awake()
    {
        _navMeshSurface = GetComponent<NavMeshSurface>();
        _mapGenerator = GetComponent<MapGenerator>();
        _aiSpawnManager = GetComponent<AISpawnManager>();
    }

    private void Start()
    {
        _mapGenerator.InitGenerator(floorSlot, dungeonGenerateDataSo);
        StartCoroutine(GenerateMapSequence());
    }

    private IEnumerator GenerateMapSequence()
    {
        // 1단계 : 맵 생성
        GenerateMap();
        OnGeneratedDungeon?.Invoke();
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
        
        // 4단계 : PathFinder 생성
        var mapData = _mapGenerator.GetMapData();
        _pathfinder = new GridPathfinder(mapData);
        
        _pathfinder.AllowDiagonalMovement = false; // 4방향 이동만
        
        // 5단계 : A.I. 생성
        _aiSpawnManager.Init(_pathfinder);
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
        FixedGrid = _mapGenerator.GenerateMap();
    }

    private void GenerateRoom()
    {
        MapData mapData = _mapGenerator.GetMapData();
        Queue<DungeonRoomDataSO> essentialRoomQueue = new Queue<DungeonRoomDataSO>(dungeonRoomListDataSo.essentialRoom);
        List<DungeonRoomDataSO> subRoomList = new List<DungeonRoomDataSO>(dungeonRoomListDataSo.subRoom);

        foreach (var room in mapData.floorList)
        {
            DungeonRoomDataSO targetRoomData = null;

            if (essentialRoomQueue.Count > 0)
            {
                targetRoomData = essentialRoomQueue.Dequeue();
            }
            else if (subRoomList.Count > 0)
            {
                targetRoomData = subRoomList[UnityEngine.Random.Range(0, subRoomList.Count)];
            }
        
            if (targetRoomData != null)
            {
                InstantiateBuilding(targetRoomData, room);
            }
        }
        
        
    }

    private void InstantiateBuilding(DungeonRoomDataSO targetBuilding, RectInt room)
    {
        Vector2Int roomStartPos = new Vector2Int(room.position.x, room.position.y);
        foreach (var objectData in targetBuilding.props)
        {
            var buildPos = objectData.pos + roomStartPos;
            var buildObj = objectData.buildObject;
            var dir = objectData.dir;
            var level = objectData.level;

            BaseGridBuildSystem.Instance.ObjectToPlace = buildObj;
            BaseGridBuildSystem.Instance.PlaceTile(buildPos.x, buildPos.y, dir, level, true);
        }
        BaseGridBuildSystem.Instance.ObjectToPlace = null;
    }
}