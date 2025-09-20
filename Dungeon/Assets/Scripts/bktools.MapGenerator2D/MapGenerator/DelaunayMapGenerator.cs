using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using bkTools;
using Random = UnityEngine.Random;

public class DelaunayMapGenerator : BaseMapGenerator
{
    [Header("Delaunay 설정")]
    [SerializeField] protected int minRoomSize = 9; // 최소 방 크기
    [SerializeField] protected int maxRoomSize = 12; // 최대 방 크기
    protected int roomCount; // 방의 개수
    
    public DelaunayMapGenerator(Transform slot, TileMappingDataSO tileMappingData,
        Vector2Int gridSize, Vector3 cubeSize, int minRoomSize, int maxRoomSize) : base(slot, tileMappingData, gridSize, cubeSize)
    {
        this.minRoomSize = minRoomSize;
        this.maxRoomSize = maxRoomSize;
    }
    
    protected override void InitializeGenerator()
    {
        int avgRoomSize = (minRoomSize + maxRoomSize) / 2;
        int spacing = 3; // 벽+복도
        int effectiveSize = avgRoomSize + spacing;
    
        int roomsX = gridSize.x / effectiveSize;
        int roomsY = gridSize.y / effectiveSize;
    
        roomCount = Mathf.Max(8, Mathf.RoundToInt(roomsX * roomsY * 0.3f));
    
        Debug.Log($"방 개수: {roomCount}개");
        
        // Delaunay는 기본적으로 A* 경로 찾기를 사용
        pathType = PathType.AStar;
    }
    
    [ContextMenu("Create Map")]
    public override void GenerateMap(int seed)
    {
        Random.InitState(seed);
        InitializeGrid();
        PlaceRooms();
        CreatePathByTriangulate();
        //CreatePathByEdgeProjection();
        ExpandPath();
        BuildWalls();
        
        RenderGrid();
        
        // 맵 데이터 설정
        var mapData = GetMapData();
        mapData.roomCount = _floorList.Count;
        
        OnMapGenerationComplete();
    }
    
    private void PlaceRooms()
    {
        const int MaxAttemptsPerRoom = 5;
        int maxAttempts = roomCount * MaxAttemptsPerRoom;
        int roomsPlaced = 0;

        int currentMinSize = minRoomSize;
        int currentMaxSize = maxRoomSize;

        while (roomsPlaced < roomCount && maxAttempts > 0)
        {
            RectInt newRoom = GenerateRoom(currentMinSize, currentMaxSize);

            // 경계 밖으로 벗어나면 시도 무효
            if (newRoom.x < margin || newRoom.x >= gridSize.x - margin ||
                newRoom.y < margin || newRoom.y >= gridSize.y - margin)
            {
                maxAttempts--;
                continue;
            }

            if (!DoesOverlap(newRoom))
            {
                _floorList.Add(newRoom);
                PlaceRoomTiles(newRoom);

                roomsPlaced++;
            }

            maxAttempts--;

            // 점진적으로 방 크기 축소
            if (maxAttempts == roomCount * 3)
            {
                currentMaxSize = Mathf.Max(minRoomSize + 1, currentMaxSize - 1);
            }
            else if (maxAttempts == roomCount)
            {
                currentMinSize = Mathf.Max(2, currentMinSize - 1);
            }
        }

        if (roomsPlaced < roomCount)
        {
            Debug.LogWarning($"⚠️ {roomCount}개 중 {roomsPlaced}개 방만 배치되었습니다.\n" +
                             $"- gridSize: {gridSize.x}x{gridSize.y}, " +
                             $"- minSize: {minRoomSize}, maxSize: {maxRoomSize}\n" +
                             $"- 남은 공간 부족일 수 있습니다.");
        }
    }

    private RectInt GenerateRoom(int minSize, int maxSize)
    {
        int width = Random.Range(minSize, maxSize + 1);
        int height = Random.Range(minSize, maxSize + 1);
        int x = Random.Range(1, gridSize.x - width - 1);
        int y = Random.Range(1, gridSize.y - height - 1);

        return new RectInt(x, y, width, height);
    }

    private bool DoesOverlap(RectInt room)
    {
        foreach (var existing in _floorList)
        {
            RectInt expanded = new RectInt(
                existing.x - margin, existing.y - margin,
                existing.width + margin*2, existing.height + margin*2
            );

            if (room.Overlaps(expanded))
                return true;
        }
        return false;
    }

    private void PlaceRoomTiles(RectInt room)
    {
        Vector2Int center = new Vector2Int(
            room.x + room.width / 2,
            room.y + room.height / 2
        );

        for (int x = room.x; x < room.x + room.width; x++)
        {
            for (int y = room.y; y < room.y + room.height; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                _grid[x, y] = (pos == center) ? CellType.FloorCenter : CellType.Floor;
            }
        }
    }
    
    private Vector3 ConvertGridPos(Vector2 pos)
    {
        Vector3 position = new Vector3(pos.x * cubeSize.x, 0, pos.y * cubeSize.z);
        return position;
    }
}
