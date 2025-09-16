using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// BSP(Binary Space Partitioning) Full 알고리즘을 사용한 던전 맵 생성기
/// 분할된 영역 전체를 방으로 사용합니다.
/// </summary>
public class BSPDungeonMapGeneratorFull : BaseMapGenerator
{
    [Header("BSP Full 설정")]
    [SerializeField] private int minSplitSize = 6;   // 최소 분할 크기
    [SerializeField] private int maxDepth = 5;
    
    private List<RoomNode> _leafNodes;
    
    /// <summary>
    /// 모든 매개변수를 지정하는 생성자
    /// </summary>
    /// <param name="slot">타일을 생성할 부모 Transform</param>
    /// <param name="tileMappingData">타일 매핑 데이터</param>
    /// <param name="gridSize">그리드 크기</param>
    /// <param name="cubeSize">큐브 크기</param>
    /// <param name="seed">시드 값</param>
    /// <param name="minSplitSize">최소 분할 크기</param>
    /// <param name="maxDepth">최대 분할 깊이</param>
    public BSPDungeonMapGeneratorFull(Transform slot, TileMappingDataSO tileMappingData,
        Vector2Int gridSize, Vector3 cubeSize, int minSplitSize, int maxDepth) : base(slot, tileMappingData, gridSize, cubeSize)
    {
        this.minSplitSize = minSplitSize;
        this.maxDepth = maxDepth;
    }
    
    protected override void InitializeGenerator()
    {
        _leafNodes = new List<RoomNode>();
        
        pathType = PathType.Straight;
    }
    
    [ContextMenu("Create Map")]
    public override void GenerateMap()
    {
        InitializeGrid();
        
        // 리프 노드 초기화
        if (_leafNodes == null)
            _leafNodes = new List<RoomNode>();
        else
            _leafNodes.Clear();
        
        RoomNode rootNode = new RoomNode(new RectInt(0, 0, gridSize.x, gridSize.y));
        SplitNode(rootNode, 0);
        PlaceRooms(rootNode);
        CreatePathByTriangulate();
        
        foreach (var node in _leafNodes)
            PlaceRoomOnGrid(node.RoomRect.position, node.RoomRect.size);
        
        BuildWalls();
        RenderGrid();
        
        // 맵 데이터 설정
        var mapData = GetMapData();
        mapData.roomCount = _leafNodes.Count;
        
        OnMapGenerationComplete();
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
            // 분할된 영역 전체를 방으로 사용
            node.RoomRect = node.NodeRect;
            _leafNodes.Add(node);
            _floorList.Add(node.RoomRect); // BaseMapGenerator의 _floorList에 방 추가
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
                if (x >= margin && x < gridSize.x - margin && y >= margin && y < gridSize.y - margin)
                {
                    bool isBorder = (x == xMin || x == xMax - 1 || y == yMin || y == yMax - 1);
                    Vector2Int pos = new Vector2Int(x, y);
                    
                    if (pos == center)
                        _grid[x, y] = CellType.FloorCenter;
                    else
                        _grid[x, y] = isBorder ? 
                            (_grid[x, y] == CellType.Path ? CellType.Path : CellType.Wall) : 
                            CellType.Floor;
                }
            }
        }
    }
    
    protected override void ClearMap()
    {
        base.ClearMap();
        
        // 리프 노드 초기화
        if (_leafNodes != null)
        {
            _leafNodes.Clear();
        }
    }
}