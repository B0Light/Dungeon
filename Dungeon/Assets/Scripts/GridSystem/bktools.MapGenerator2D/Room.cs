using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RoomInfo
{
    public Vector2Int position;  // 그리드 상의 좌측 하단 위치
    public Vector2Int size;      // 방 크기
    public Vector2Int center;    // 그리드 상의 중심점
    public Vector3 worldPosition; // 월드 좌표계에서의 좌측 하단 위치
    public Vector3 worldCenter;   // 월드 좌표계에서의 중심점

    public override string ToString()
    {
        return $"Room at {position}, size: {size}, center: {center}";
    }
}

public class RoomNode
{
    public RectInt NodeRect;
    public RectInt RoomRect;
    public RoomNode Left;
    public RoomNode Right;

    public RoomNode(RectInt rect) => NodeRect = rect;

    public Vector2Int GetRoomCenter()
    {
        if (RoomRect.width > 0 && RoomRect.height > 0)
            return new Vector2Int(RoomRect.x + (RoomRect.width - 1) / 2, RoomRect.y + (RoomRect.height - 1) / 2);

        Vector2Int center = new Vector2Int(NodeRect.x + (NodeRect.width - 1) / 2, NodeRect.y + (NodeRect.height - 1) / 2);
        if (Left != null) return Left.GetRoomCenter();
        if (Right != null) return Right.GetRoomCenter();
        return center;
    }
}

public class Room
{
    public Vector2Int Position;
    public int Width;
    public int Height;
    public RoomType Type;
    public List<Vector2Int> Doors = new List<Vector2Int>();

    public Room(Vector2Int pos, int width, int height, RoomType type)
    {
        Position = pos;
        Width = width;
        Height = height;
        Type = type;
    }
}



public enum RoomType
{
    Start,
    Normal,
    Special
}

public enum CellType
{
    Empty,
    Floor,
    FloorCenter,
    Wall,
    PathWall,
    Path,
    ExpandedPath,
    MainGate,
    SubGate,
}


