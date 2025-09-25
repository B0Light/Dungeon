using System.Collections.Generic;
using UnityEngine;

public class IsaacMapGenerator : BaseMapGenerator
{
    [Header("맵 설정")]
    public int maxRooms = 15;             // 최대 방 개수
    public int specialRoomCount = 3;      // 특수 방 개수

    private Dictionary<Vector2Int, Room> rooms = new Dictionary<Vector2Int, Room>();
    
    public IsaacMapGenerator(Transform slot, DungeonDataSO dungeonDataSo) : base(slot, dungeonDataSo) { }
    
    protected override void InitializeGenerator()
    {
        rooms = new Dictionary<Vector2Int, Room>();
        
        int spacing = 3; // 벽+복도
        int effectiveSize = _config.RoomSize + spacing;
    
        int roomsX = _config.GridSize.x / effectiveSize;
        int roomsY = _config.GridSize.y / effectiveSize;
    
        maxRooms = Mathf.Max(8, Mathf.RoundToInt(roomsX * roomsY * 0.3f));
    }
    
    [ContextMenu("Create Map")]
    public override void GenerateMap(int seed)
    {
        Random.InitState(seed);
        InitializeGrid();
        GenerateRooms();
        PlaceSpecialRooms();
        BuildWalls();
        ExpandPath();
        BuildPathWalls();
        BuildGate();
        PopulateRoomGateDirections();
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

        rooms[startPos] = new Room(startPos, _config.RoomSize, _config.RoomSize, RoomType.Start);
        PlaceRoomOnGrid(startPos, _config.RoomSize, _config.RoomSize);

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

                if (IsOutOfGrid(newPos, _config.RoomSize, _config.RoomSize, 3))
                    continue;

                // 현재 방 개수에 따라 가변 확률 조절
                float progress = (float)rooms.Count / maxRooms;
                float spawnChance = Mathf.Lerp(0.9f, 0.3f, progress); // 초기엔 90%, 점점 30%로

                if (Random.value > spawnChance)
                    continue;

                Room newRoom = new Room(newPos, _config.RoomSize, _config.RoomSize, RoomType.Normal);
                rooms[newPos] = newRoom;

                rooms[current].Doors.Add(dir);
                rooms[newPos].Doors.Add(-dir);

                PlaceRoomOnGrid(newPos, _config.RoomSize, _config.RoomSize);
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
        int gridX = (_config.GridSize.x / 2) + (roomGridPos.x * (width + spacing));
        int gridY = (_config.GridSize.y / 2) + (roomGridPos.y * (height + spacing));

        return gridX < 1 || gridX + width >= _config.GridSize.x - 1 ||
               gridY < 1 || gridY + height >= _config.GridSize.y - 1;
    }
    
    private void PlaceRoomOnGrid(Vector2Int roomPos, int width, int height)
    {
        // 그리드 좌표로 변환 (간격을 고려한 배치)
        int gridX = (_config.GridSize.x / 2) + (roomPos.x * (width + _config.Margin));
        int gridY = (_config.GridSize.y / 2) + (roomPos.y * (height + _config.Margin));

        // 방이 그리드 범위를 벗어나지 않도록 조정
        gridX = Mathf.Clamp(gridX, 1, _config.GridSize.x - width - 1);
        gridY = Mathf.Clamp(gridY, 1, _config.GridSize.y - height - 1);

        RectInt roomRect = new RectInt(gridX, gridY, width, height);
        _floorList.Add(roomRect);

        // 그리드에 방 배치
        for (int x = gridX; x < gridX + width; x++)
        {
            for (int y = gridY; y < gridY + height; y++)
            {
                if (x >= 0 && x < _config.GridSize.x && y >= 0 && y < _config.GridSize.y)
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
                CreateCorridor(gridX, gridY, width, height, door);
            }
        }
    }
    
    private void CreateCorridor(int roomX, int roomY, int width, int height, Vector2Int direction)
    {
        // 복도 길이를 spacing에 맞춰 조정
        int corridorLength = _config.Margin + 1; // 방 사이 간격만큼 복도 생성
        
        // 방의 가장자리에서 복도 시작점 계산
        int startX, startY;
        
        if (direction.x > 0) // 오른쪽
        {
            startX = roomX + width - 1;
            startY = roomY + height / 2;
        }
        else if (direction.x < 0) // 왼쪽
        {
            startX = roomX;
            startY = roomY + height / 2;
        }
        else if (direction.y > 0) // 위쪽
        {
            startX = roomX + width / 2;
            startY = roomY + height - 1;
        }
        else // 아래쪽
        {
            startX = roomX + width / 2;
            startY = roomY;
        }
        
        // 복도 끝점 계산
        int endX = startX + direction.x * corridorLength;
        int endY = startY + direction.y * corridorLength;

        _grid[startX, startY] = CellType.Gate;
        _grid[endX, endY] = CellType.Gate;
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