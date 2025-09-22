using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Isaac 스타일의 던전 맵 생성기
/// BFS 방식으로 방을 생성하고 연결합니다.
/// </summary>
public class IsaacMapGenerator : BaseMapGenerator
{
    [Header("맵 설정")]
    public int maxRooms = 15;             // 최대 방 개수
    public int specialRoomCount = 3;      // 특수 방 개수

    [Header("방 크기 (고정)")]
    public int horizontalSize = 11;       // 가로 길이
    public int verticalSize = 11;          // 세로 길이

    private Dictionary<Vector2Int, Room> rooms = new Dictionary<Vector2Int, Room>();
    
    
    public IsaacMapGenerator(Transform slot, TileMappingDataSO tileMappingData,
        Vector2Int gridSize, Vector3 cubeSize, int specialRoomCount, int horizontalSize, int verticalSize) : base(slot, tileMappingData, gridSize, cubeSize)
    {
        this.specialRoomCount = specialRoomCount;
        this.horizontalSize = horizontalSize;
        this.verticalSize = verticalSize;
    }
    
    protected override void InitializeGenerator()
    {
        rooms = new Dictionary<Vector2Int, Room>();
        
        int avgRoomSize = (horizontalSize + verticalSize) / 2;
        int spacing = 3; // 벽+복도
        int effectiveSize = avgRoomSize + spacing;
    
        int roomsX = gridSize.x / effectiveSize;
        int roomsY = gridSize.y / effectiveSize;
    
        maxRooms = Mathf.Max(8, Mathf.RoundToInt(roomsX * roomsY * 0.3f));
        
        // Isaac 스타일은 기본적으로 직선 복도를 사용
        pathType = PathType.Straight;
    }
    
    [ContextMenu("Create Map")]
    public override void GenerateMap(int seed)
    {
        Random.InitState(seed);
        InitializeGrid();
        GenerateRooms();
        PlaceSpecialRooms();
        BuildWalls();
        RenderGrid();
        
        // 맵 데이터 설정
        var mapData = GetMapData();
        mapData.roomCount = rooms.Count;
    }

    private void GenerateRooms()
    {
        rooms.Clear();

        Vector2Int startPos = Vector2Int.zero;
        Queue<Vector2Int> frontier = new Queue<Vector2Int>();
        frontier.Enqueue(startPos);

        rooms[startPos] = new Room(startPos, horizontalSize, verticalSize, RoomType.Start);
        PlaceRoomOnGrid(startPos, horizontalSize, verticalSize, true);

        System.Random prng = new System.Random(System.DateTime.Now.Millisecond);

        while (frontier.Count > 0 && rooms.Count < maxRooms)
        {
            Vector2Int current = frontier.Dequeue();

            List<Vector2Int> directions = GetDirections();
            ShuffleList(directions, prng); // 무작위 방향 순서

            foreach (var dir in directions)
            {
                Vector2Int newPos = current + dir;

                if (rooms.ContainsKey(newPos))
                    continue;

                if (IsOutOfGrid(newPos, horizontalSize, verticalSize, 3))
                    continue;

                // 현재 방 개수에 따라 가변 확률 조절
                float progress = (float)rooms.Count / maxRooms;
                float spawnChance = Mathf.Lerp(0.9f, 0.3f, progress); // 초기엔 90%, 점점 30%로

                if (Random.value > spawnChance)
                    continue;

                Room newRoom = new Room(newPos, horizontalSize, verticalSize, RoomType.Normal);
                rooms[newPos] = newRoom;

                rooms[current].Doors.Add(dir);
                rooms[newPos].Doors.Add(-dir);

                PlaceRoomOnGrid(newPos, horizontalSize, verticalSize, false);
                frontier.Enqueue(newPos);

                if (rooms.Count >= maxRooms)
                    break;
            }
        }

        // 예외 처리: 방이 너무 적게 생성되면 재생성
        if (rooms.Count < 5)
        {
            Debug.LogWarning("방 개수가 너무 적어 맵을 다시 생성합니다.");
            GenerateRooms(); // 재귀 호출
        }
    }
    
    private bool IsOutOfGrid(Vector2Int roomGridPos, int width, int height, int spacing)
    {
        int gridX = (gridSize.x / 2) + (roomGridPos.x * (width + spacing));
        int gridY = (gridSize.y / 2) + (roomGridPos.y * (height + spacing));

        return gridX < 1 || gridX + width >= gridSize.x - 1 ||
               gridY < 1 || gridY + height >= gridSize.y - 1;
    }
    
    private void PlaceRoomOnGrid(Vector2Int roomPos, int width, int height, bool isStartRoom)
    {
        // 방들 사이에 간격을 두기 위해 spacing 추가
        int spacing = 3; // 방 사이 간격 (복도 공간)
        
        // 그리드 좌표로 변환 (간격을 고려한 배치)
        int gridX = (gridSize.x / 2) + (roomPos.x * (width + spacing));
        int gridY = (gridSize.y / 2) + (roomPos.y * (height + spacing));

        // 방이 그리드 범위를 벗어나지 않도록 조정
        gridX = Mathf.Clamp(gridX, 1, gridSize.x - width - 1);
        gridY = Mathf.Clamp(gridY, 1, gridSize.y - height - 1);

        RectInt roomRect = new RectInt(gridX, gridY, width, height);
        _floorList.Add(roomRect);

        // 그리드에 방 배치
        for (int x = gridX; x < gridX + width; x++)
        {
            for (int y = gridY; y < gridY + height; y++)
            {
                if (x >= 0 && x < gridSize.x && y >= 0 && y < gridSize.y)
                {
                    if (x == gridX + (width - 1) / 2 && y == gridY + (height - 1) / 2)
                        _grid[x, y] = CellType.FloorCenter;
                    else
                        _grid[x, y] = CellType.Floor;
                }
            }
        }

        // 복도 생성 (문이 있는 방향)
        if (rooms.ContainsKey(roomPos))
        {
            foreach (var door in rooms[roomPos].Doors)
            {
                CreateCorridor(gridX, gridY, width, height, door, spacing);
            }
        }
    }
    
    private void CreateCorridor(int roomX, int roomY, int width, int height, Vector2Int direction, int spacing)
    {
        // 복도 길이를 spacing에 맞춰 조정
        int corridorLength = spacing - 1; // 방 사이 간격만큼 복도 생성
        
        // 방의 가장자리에서 복도 시작점 계산
        int startX, startY;
        
        if (direction.x > 0) // 오른쪽
        {
            startX = roomX + width;
            startY = roomY + height / 2;
        }
        else if (direction.x < 0) // 왼쪽
        {
            startX = roomX - 1;
            startY = roomY + height / 2;
        }
        else if (direction.y > 0) // 위쪽
        {
            startX = roomX + width / 2;
            startY = roomY + height;
        }
        else // 아래쪽
        {
            startX = roomX + width / 2;
            startY = roomY - 1;
        }
        
        // 복도 끝점 계산
        int endX = startX + direction.x * corridorLength;
        int endY = startY + direction.y * corridorLength;
        
        // BaseMapGenerator의 경로 생성 메서드 사용
        CreatePathBetweenPoints(new Vector2Int(startX, startY), new Vector2Int(endX, endY));
    }

    void PlaceSpecialRooms()
    {
        List<Vector2Int> candidates = new List<Vector2Int>(rooms.Keys);

        // 시작 방 제외
        candidates.Remove(Vector2Int.zero);

        for (int i = 0; i < specialRoomCount && candidates.Count > 0; i++)
        {
            int index = Random.Range(0, candidates.Count);
            Vector2Int pos = candidates[index];
            candidates.RemoveAt(index);

            rooms[pos].Type = RoomType.Special;
        }
    }

    List<Vector2Int> GetDirections()
    {
        return new List<Vector2Int>
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };
    }
    
    private void ShuffleList<T>(List<T> list, System.Random rng)
    {
        int n = list.Count;
        while (n > 1)
        {
            int k = rng.Next(n--);
            (list[n], list[k]) = (list[k], list[n]);
        }
    }
}