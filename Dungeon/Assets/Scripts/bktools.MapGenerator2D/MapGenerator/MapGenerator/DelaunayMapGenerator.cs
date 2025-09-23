using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using bkTools;
using Random = UnityEngine.Random;

public class DelaunayMapGenerator : BaseMapGenerator
{
    [Header("Delaunay 설정")]
    private int _roomCount;
    
    public DelaunayMapGenerator(Transform slot, DungeonDataSO dungeonDataSo) : base(slot, dungeonDataSo) { }
    
    protected override void InitializeGenerator()
    {
        int effectiveSize = _config.RoomSize + _config.Margin;
    
        int roomsX = _config.GridSize.x / effectiveSize;
        int roomsY = _config.GridSize.y / effectiveSize;
    
        _roomCount = Mathf.Max(8, Mathf.RoundToInt(roomsX * roomsY * 0.5f));
    }
    
    [ContextMenu("Create Map")]
    public override void GenerateMap(int seed)
    {
        Random.InitState(seed);
        InitializeGrid();
        PlaceRooms();
        BuildWalls();
        CreatePathByTriangulate();
        ExpandPath();
        BuildPathWalls();
        BuildGate();
        PopulateRoomGateDirections();
        RenderGrid();
        
        // 맵 데이터 설정
        var mapData = GetMapData();
        mapData.roomCount = _floorList.Count;
    }
    
    private void PlaceRooms()
    {
        const int MaxAttemptsPerRoom = 5;
        int maxAttempts = _roomCount * MaxAttemptsPerRoom;
        int roomsPlaced = 0;

        while (roomsPlaced < _roomCount && maxAttempts > 0)
        {
            RectInt newRoom = GenerateRoom(_config.RoomSize);

            // 경계 밖으로 벗어나면 시도 무효
            if (newRoom.x < _config.Margin || newRoom.x >= _config.GridSize.x - _config.Margin ||
                newRoom.y < _config.Margin || newRoom.y >= _config.GridSize.y - _config.Margin)
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
        }

        if (roomsPlaced < _roomCount)
        {
            Debug.LogWarning($"⚠️ {_roomCount}개 중 {roomsPlaced}개 방만 배치되었습니다.\n" +
                             $"- _config.GridSize: {_config.GridSize.x}x{_config.GridSize.y}, " +
                             $"- size: {_config.RoomSize}\n" +
                             $"- 남은 공간 부족일 수 있습니다.");
        }
    }

    private RectInt GenerateRoom(int size)
    {
        int width = size;
        int height = size;
        int x = Random.Range(1, _config.GridSize.x - width - 1);
        int y = Random.Range(1, _config.GridSize.y - height - 1);

        return new RectInt(x, y, width, height);
    }

    private bool DoesOverlap(RectInt room)
    {
        foreach (var existing in _floorList)
        {
            RectInt expanded = new RectInt(
                existing.x - _config.Margin, existing.y - _config.Margin,
                existing.width + _config.Margin*2, existing.height + _config.Margin*2
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
        Vector3 position = new Vector3(pos.x * _config.CubeSize.x, 0, pos.y * _config.CubeSize.z);
        return position;
    }
}
