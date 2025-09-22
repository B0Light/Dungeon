using System.Collections.Generic;
using UnityEngine;

public class BSPDungeonMapGeneratorFull : BaseMapGenerator
{
    [Header("BSP Full 설정")]
    [SerializeField] private int minSplitSize = 6;   // 최소 분할 크기
    [SerializeField] private int maxDepth = 5;
    
    private List<RoomNode> _leafNodes;
    
    public BSPDungeonMapGeneratorFull(Transform slot, DungeonDataSO dungeonDataSo) : base(slot, dungeonDataSo) { }
    
    protected override void InitializeGenerator()
    {
        _leafNodes = new List<RoomNode>();
    }
    
    public override void GenerateMap(int seed)
    {
        Random.InitState(seed);
        InitializeGrid();
        
        // 리프 노드 초기화
        if (_leafNodes == null)
            _leafNodes = new List<RoomNode>();
        else
            _leafNodes.Clear();
        
        RoomNode rootNode = new RoomNode(new RectInt(0, 0, _config.GridSize.x, _config.GridSize.y));
        SplitNode(rootNode, 0);
        PlaceRooms(rootNode);
        BuildWalls();
        ConnectRooms(rootNode);
        
        foreach (var node in _leafNodes)
            PlaceRoomOnGrid(node.RoomRect.position, node.RoomRect.size);
        
        ExpandPath();
        BuildPathWalls();
        BuildGate();
        RenderGrid();
        
        // 맵 데이터 설정
        var mapData = GetMapData();
        mapData.roomCount = _leafNodes.Count;
    }
    
    bool SplitNode(RoomNode node, int depth)
    {
        if (depth >= maxDepth) return false;

        bool splitHorizontally;

        // 가로, 세로 길이에 따라 분할 방향 결정
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
            // 가로 세로가 같으면 랜덤
            splitHorizontally = Random.value < 0.5f;
        }

        if (splitHorizontally && node.NodeRect.height < minSplitSize * 2) return false;
        if (!splitHorizontally && node.NodeRect.width < minSplitSize * 2) return false;

        if (splitHorizontally)
        {
            int splitY = Random.Range(minSplitSize, node.NodeRect.height - minSplitSize);
            node.Left = new RoomNode(new RectInt(node.NodeRect.x, node.NodeRect.y, node.NodeRect.width, splitY));
            node.Right = new RoomNode(new RectInt(node.NodeRect.x, node.NodeRect.y + splitY, node.NodeRect.width, node.NodeRect.height - splitY));
        }
        else
        {
            int splitX = Random.Range(minSplitSize, node.NodeRect.width - minSplitSize);
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
            // 분할된 영역 전체를 방으로 사용 + 마진 적용 
            node.RoomRect = new RectInt(node.NodeRect.xMin, node.NodeRect.yMin, node.NodeRect.width -3, node.NodeRect.height -3);
            _leafNodes.Add(node);
            _floorList.Add(node.RoomRect);
        }
    }
    
    private void ConnectRooms(RoomNode node)
    {
        if (node.Left != null && node.Right != null)
        {
            ConnectRooms(node.Left);
            ConnectRooms(node.Right);
            Vector2Int pointA = node.Left.GetRoomCenter();
            Vector2Int pointB = node.Right.GetRoomCenter();
                
            // BaseMapGenerator의 경로 생성 메서드 사용
            CreatePathBetweenPoints(pointA, pointB);
        }
    }

    private void PlaceRoomOnGrid(Vector2Int location, Vector2Int size)
    {
        int xMin = location.x;
        int yMin = location.y;
        int xMax = location.x + size.x;
        int yMax = location.y + size.y;

        // 방의 중심 위치 계산 (정확한 중심 위치)
        Vector2Int center = new Vector2Int(
            location.x + (size.x - 1) / 2,
            location.y + (size.y - 1) / 2
        );

        for (int x = xMin; x < xMax; x++)
        {
            for (int y = yMin; y < yMax; y++)
            {
                if (x >= _config.Margin && x < _config.GridSize.x - _config.Margin && y >= _config.Margin && y < _config.GridSize.y - _config.Margin)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    
                    if (pos == center)
                        _grid[x, y] = CellType.FloorCenter;
                    else
                        _grid[x, y] = CellType.Floor;
                }
            }
        }
    }
}