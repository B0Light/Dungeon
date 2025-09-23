using System.Collections.Generic;
using UnityEngine;

public class BSPDungeonMapGenerator : BaseMapGenerator
{
    [Header("BSP 설정")]
    private int _maxDepth = 5;
    
    private List<RoomNode> _leafNodes;
    
    public BSPDungeonMapGenerator(Transform slot, DungeonDataSO  dungeonDataSo) : base(slot, dungeonDataSo) { }
    
    protected override void InitializeGenerator()
    {
        _leafNodes = new List<RoomNode>();
    }
    
    [ContextMenu("Create Map")]
    public override void GenerateMap(int seed)
    {
        Random.InitState(seed);
        InitializeGrid();
        
        if (_leafNodes == null)
            _leafNodes = new List<RoomNode>();
        else
            _leafNodes.Clear();
        
        RoomNode rootNode = new RoomNode(new RectInt(0, 0, _config.GridSize.x, _config.GridSize.y));
        SplitNode(rootNode, 0);
        PlaceRooms(rootNode);
        BuildWalls();
        CreatePathByTriangulate();
        
        foreach (var node in _leafNodes) 
            PlaceRoomOnGrid(node.RoomRect.position, node.RoomRect.size);
        
        ExpandPath();
        BuildPathWalls();
        BuildGate();
        PopulateRoomGateDirections();
        RenderGrid();
        
        // 맵 데이터 설정
        var mapData = GetMapData();
        mapData.roomCount = _leafNodes.Count;
    }
    
    bool SplitNode(RoomNode node, int depth)
    {
        if (depth >= _maxDepth) return false;

        bool splitHorizontally;

        // 가로/세로 길이에 따라 분할 방향 결정
        if (node.NodeRect.width > node.NodeRect.height)
        {
            splitHorizontally = false; // 세로 분할
        }
        else if (node.NodeRect.height > node.NodeRect.width)
        {
            splitHorizontally = true;  // 가로 분할
        }
        else
        {
            // 길이가 같으면 랜덤
            splitHorizontally = Random.value < 0.5f;
        }

        // 분할 가능한 크기인지 확인
        if (splitHorizontally && node.NodeRect.height < _config.RoomSize * 2) return false;
        if (!splitHorizontally && node.NodeRect.width < _config.RoomSize * 2) return false;

        if (splitHorizontally)
        {
            int splitY = Random.Range(_config.RoomSize, node.NodeRect.height - _config.RoomSize);
            node.Left = new RoomNode(new RectInt(node.NodeRect.x, node.NodeRect.y, node.NodeRect.width, splitY));
            node.Right = new RoomNode(new RectInt(node.NodeRect.x, node.NodeRect.y + splitY, node.NodeRect.width, node.NodeRect.height - splitY));
        }
        else
        {
            int splitX = Random.Range(_config.RoomSize, node.NodeRect.width - _config.RoomSize);
            node.Left = new RoomNode(new RectInt(node.NodeRect.x, node.NodeRect.y, splitX, node.NodeRect.height));
            node.Right = new RoomNode(new RectInt(node.NodeRect.x + splitX, node.NodeRect.y, node.NodeRect.width - splitX, node.NodeRect.height));
        }

        SplitNode(node.Left, depth + 1);
        SplitNode(node.Right, depth + 1);
        return true;
    }

    void PlaceRooms(RoomNode node)
    {
        if (node.Left != null || node.Right != null)
        {
            if (node.Left != null) PlaceRooms(node.Left);
            if (node.Right != null) PlaceRooms(node.Right);
        }
        else
        {
            int roomX = Random.Range(node.NodeRect.xMin, node.NodeRect.xMax - _config.RoomSize);
            int roomY = Random.Range(node.NodeRect.yMin, node.NodeRect.yMax - _config.RoomSize);

            node.RoomRect = new RectInt(roomX, roomY, _config.RoomSize, _config.RoomSize);
            _leafNodes.Add(node);
            _floorList.Add(node.RoomRect); // BaseMapGenerator의 _floorList에 방 추가
        }
    }

    private void PlaceRoomOnGrid(Vector2Int location, Vector2Int size)
    {
        Vector2Int center = new Vector2Int(
            location.x + (size.x - 1) / 2,
            location.y + (size.y - 1) / 2
        );
        
        for (int x = location.x; x < location.x + size.x; x++)
        {
            for (int y = location.y; y < location.y + size.y; y++)
            {
                if (x >= _config.Margin && x < _config.GridSize.x - _config.Margin && y >= _config.Margin && y < _config.GridSize.y - _config.Margin)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    _grid[x, y] = (pos == center) ? CellType.FloorCenter : CellType.Floor;
                }
            }
        }
    }
}