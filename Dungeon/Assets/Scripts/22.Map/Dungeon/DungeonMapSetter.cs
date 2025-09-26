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
}