using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using bkTools;
using Random = UnityEngine.Random;

/// <summary>
/// Delaunay 삼각분할과 Kruskal 알고리즘을 사용한 던전 맵 생성기
/// </summary>
public class DelaunayMapGenerator : BaseMapGenerator
{
    [Header("Delaunay 설정")]
    [SerializeField] protected int minRoomSize = 9; // 최소 방 크기
    [SerializeField] protected int maxRoomSize = 12; // 최대 방 크기
    protected int roomCount; // 방의 개수
    

    
    protected Delaunay2D _delaunay;
    
    /// <summary>
    /// 기본 생성자
    /// </summary>
    /// <param name="slot">타일을 생성할 부모 Transform</param>
    /// <param name="tileMappingData">타일 매핑 데이터</param>
    public DelaunayMapGenerator(Transform slot, TileMappingDataSO tileMappingData) : base(slot, tileMappingData)
    {
    }
    
    /// <summary>
    /// 그리드 크기와 큐브 크기를 지정하는 생성자
    /// </summary>
    /// <param name="slot">타일을 생성할 부모 Transform</param>
    /// <param name="tileMappingData">타일 매핑 데이터</param>
    /// <param name="gridSize">그리드 크기</param>
    /// <param name="cubeSize">큐브 크기</param>
    public DelaunayMapGenerator(Transform slot, TileMappingDataSO tileMappingData, Vector2Int gridSize, Vector3 cubeSize) : base(slot, tileMappingData, gridSize, cubeSize)
    {
    }
    
    /// <summary>
    /// 모든 매개변수를 지정하는 생성자
    /// </summary>
    /// <param name="slot">타일을 생성할 부모 Transform</param>
    /// <param name="tileMappingData">타일 매핑 데이터</param>
    /// <param name="gridSize">그리드 크기</param>
    /// <param name="cubeSize">큐브 크기</param>
    /// <param name="seed">시드 값</param>
    /// <param name="minRoomSize">최소 방 크기</param>
    /// <param name="maxRoomSize">최대 방 크기</param>
    /// <param name="pathValue">경로 생성 확률</param>
    public DelaunayMapGenerator(Transform slot, TileMappingDataSO tileMappingData, Vector2Int gridSize, Vector3 cubeSize, int seed, int minRoomSize, int maxRoomSize, float pathValue) : base(slot, tileMappingData, gridSize, cubeSize)
    {
        this.seed = seed;
        this.minRoomSize = minRoomSize;
        this.maxRoomSize = maxRoomSize;
        this.pathValue = pathValue;
    }
    
    protected override void InitializeGenerator()
    {
        int avgRoomSize = (minRoomSize + maxRoomSize) / 2;
        int spacing = 3; // 벽+복도
        int effectiveSize = avgRoomSize + spacing;
    
        int roomsX = gridSize.x / effectiveSize;
        int roomsY = gridSize.y / effectiveSize;
    
        roomCount = Mathf.Max(8, Mathf.RoundToInt(roomsX * roomsY * 0.5f));
    
        Debug.Log($"방 개수: {roomCount}개");
        
        // Delaunay는 기본적으로 A* 경로 찾기를 사용
        pathType = PathType.AStar;
    }
    
    [ContextMenu("Create Map")]
    public override void GenerateMap()
    {
        InitializeGrid();
        PlaceRooms();
        Triangulate();
        CreatePath();
        ExpandPath();
        BuildWalls();
        
        RenderGrid();
        
        // 맵 데이터 설정
        var mapData = GetMapData();
        mapData.roomCount = _floorList.Count;
        mapData.seed = seed;
        
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
            if (newRoom.width >= gridSize.x - 2 || newRoom.height >= gridSize.y - 2)
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
            // 1칸 여유 공간을 두고 겹침 검사
            RectInt expanded = new RectInt(
                existing.x - 1, existing.y - 1,
                existing.width + 2, existing.height + 2
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



    
    private void Triangulate()
    {
        if (_floorList.Count < 3) return;
        
        List<Vertex> vertices = new List<Vertex>();
        
        vertices.AddRange(_floorList.Select(floor => 
            new Vertex<RectInt>(floor.position + ((Vector2)floor.size) / 2, floor)));

        _delaunay = Delaunay2D.Triangulate(vertices);
    }
    
    private void CreatePath()
    {
        // BaseMapGenerator의 Delaunay 경로 생성 메서드 사용
        CreateDelaunayPaths(_delaunay, pathValue);
    }
    
    private Vector3 ConvertGridPos(Vector2 pos)
    {
        Vector3 position = new Vector3(pos.x * cubeSize.x, 0, pos.y * cubeSize.z);
        return position;
    }
    

}
