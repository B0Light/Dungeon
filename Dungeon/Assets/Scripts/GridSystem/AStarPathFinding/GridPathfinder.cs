using System.Collections.Generic;
using UnityEngine;

public class GridPathfinder : AStarPathfindingBase<GridCell>
{
    private FixedGridXZ<GridCell> _fixedGrid;
    private GridCell _goalNode;

    private readonly CellType[,] _cellTypeGrid; 
    private Vector2Int _gridSize;
    private GridCell[,] _nodeGrid;
    
    // 이동 방향 정의 (4방향 또는 8방향) - from MapGridPathfinder
    private static readonly Vector2Int[] DirectionsCardinal = 
    {
        Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
    };
    
    private static readonly Vector2Int[] DirectionsDiagonal = 
    {
        Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right,
        new Vector2Int(1, 1), new Vector2Int(1, -1), new Vector2Int(-1, 1), new Vector2Int(-1, -1)
    };

    public bool AllowDiagonalMovement { get; set; } = false;

    
    public GridPathfinder(MapData mapData)
    {
        _cellTypeGrid = mapData.grid;
        _gridSize = mapData.mapConfig.GridSize;
        _nodeGrid = new GridCell[_gridSize.x, _gridSize.y];
        InitializeNodeGrid();
    } 
    
    public GridPathfinder() { }

    public List<GridCell> NavigatePath(Vector2Int start, Vector2Int goal)
    {
        if (_nodeGrid == null)
        {
            _fixedGrid = GridBuildingSystem.Instance.GetGrid();
            _gridSize = new Vector2Int(_fixedGrid.Width, _fixedGrid.Height);
            _nodeGrid = new GridCell[_gridSize.x, _gridSize.y];
            foreach (GridCell obj in _fixedGrid.GetAllGridObjects())
            {
                _nodeGrid[obj.Position.x, obj.Position.y] = obj;
            }
        }

        GridCell startNode = GetNode(start);
        _goalNode = GetNode(goal);
        
        if (!IsValidPosition(start) || !IsValidPosition(goal))
        {
            Debug.LogWarning($"Invalid positions: Start({start}) or Goal({goal})");
            return null;
        }

        if (!startNode.IsWalkable || !_goalNode.IsWalkable)
        {
            Debug.LogWarning($"Unwalkable positions: Start({start}:{startNode.CellType}) or Goal({goal}:{_goalNode.CellType})");
            return null;
        }

        ResetNodeCosts();
        startNode.GCost = 0;
        startNode.HCost = GetDistance(startNode, _goalNode);

        List<GridCell> path = FindPath(startNode, _goalNode);
        
        return path;
    }

    private void InitializeNodeGrid()
    {
        for (int x = 0; x < _gridSize.x; x++)
        {
            for (int y = 0; y < _gridSize.y; y++)
            {
                if (_cellTypeGrid != null)
                {
                    _nodeGrid[x, y] = new GridCell(x, y, _cellTypeGrid[x, y]);
                }
            }
        }
    }

    private void ResetNodeCosts()
    {
        for (int x = 0; x < _gridSize.x; x++)
        {
            for (int y = 0; y < _gridSize.y; y++)
            {
                var node = _nodeGrid[x, y];
                if (node != null)
                {
                    node.GCost = float.MaxValue;
                    node.HCost = 0;
                    node.Parent = null;
                }
            }
        }
    }

    protected override IEnumerable<GridCell> GetNeighbors(GridCell node)
    {
        var directions = AllowDiagonalMovement ? DirectionsDiagonal : DirectionsCardinal;

        foreach (var direction in directions)
        {
            Vector2Int neighborPos = node.Position + direction;
            
            if (IsValidPosition(neighborPos))
            {
                var neighborNode = GetNode(neighborPos);
                if (neighborNode == null) continue; // Safety check

                if (_fixedGrid != null)
                {
                    GridCell currentNeighborGrid = _fixedGrid.GetGridObject(neighborPos.x, neighborPos.y);
                    if (currentNeighborGrid?.GetTileType() == TileType.Road) 
                    {
                        yield return currentNeighborGrid;
                    }
                    
                    if ((currentNeighborGrid?.GetTileType() == TileType.Headquarter ||
                         currentNeighborGrid?.GetTileType() == TileType.Attraction ||
                         currentNeighborGrid?.GetTileType() == TileType.MajorFacility)
                        && currentNeighborGrid.Equals(_goalNode)) // Check against _goalNode here
                    {
                        Vector2Int attractionOrigin = currentNeighborGrid.GetEntrancePosition();
                        if (attractionOrigin == neighborPos)
                        {
                            BuildObjData.Dir objectDirection = currentNeighborGrid.GetDirection();

                            if (objectDirection == ConvertToConnectDirection(direction))
                            {
                                yield return currentNeighborGrid;
                            }
                        }
                    }
                }
                else if (neighborNode.IsWalkable)
                {
                    yield return neighborNode;
                }
            }
        }
    }
    
    protected override float GetDistance(GridCell a, GridCell b)
    {
        int distX = Mathf.Abs(a.GetEntrancePosition().x - b.GetEntrancePosition().x);
        int distY = Mathf.Abs(a.GetEntrancePosition().y - b.GetEntrancePosition().y);
        
        if (AllowDiagonalMovement)
        {
            int diagonal = Mathf.Min(distX, distY);
            int straight = Mathf.Max(distX, distY) - diagonal;
            return straight + diagonal * 1.414f;
        }
        // use Manhattan distance
        else
        {
            return distX + distY;
        }
    }

    // 셀 타입에 따른 이동 비용 가중치를 적용
    protected override float GetMovementCost(GridCell from, GridCell to)
    {
        float baseCost = GetDistance(from, to);

        // 타일/셀 타입별 가중치. 필요 시 외부 설정으로 분리 가능
        float terrainMultiplier = 1f;

        // 우선 순위: 배치 오브젝트의 타일 타입 > 셀 자체의 CellType
        TileType? tileType = to.GetTileType();
        if (tileType.HasValue)
        {
            switch (tileType.Value)
            {
                case TileType.Road:
                    terrainMultiplier = 0.9f; // 도로는 약간 빠르게
                    break;
                case TileType.Headquarter:
                case TileType.Attraction:
                case TileType.MajorFacility:
                    terrainMultiplier = 1.0f; // 시설 내부 진입은 기본
                    break;
                case TileType.Tree:
                    terrainMultiplier = 2.0f; // 통과 가능하다면 높은 비용
                    break;
                default:
                    terrainMultiplier = 1.0f;
                    break;
            }
        }
        else
        {
            // GridCell의 논리적 CellType 기반 가중치
            switch (to.CellType)
            {
                case CellType.Floor:
                case CellType.FloorCenter:
                case CellType.Path:
                    terrainMultiplier = 0.5f;
                    break;
                case CellType.ExpandedPath:
                    terrainMultiplier = 0.95f;
                    break;
                case CellType.MainGate:
                    terrainMultiplier = 0.7f;
                    break;
                case CellType.SubGate:
                    terrainMultiplier = 1.0f;
                    break;
                case CellType.Wall:
                case CellType.PathWall:
                case CellType.Empty:
                    // 이 경우 보통 IsWalkable이 false라 이 함수까지 오지 않지만, 안전상 높은 비용 부여
                    terrainMultiplier = 10f;
                    break;
                default:
                    terrainMultiplier = 1.0f;
                    break;
            }
        }

        // 대각 이동 보정: 기본 거리에서 이미 1.414 적용됨. 필요 시 추가 계수 조정 가능
        return baseCost * terrainMultiplier;
    }

    private static BuildObjData.Dir ConvertToConnectDirection(Vector2Int direction)
    {
        if (direction == new Vector2Int(0, 1)) return BuildObjData.Dir.Down;
        if (direction == new Vector2Int(-1, 0)) return BuildObjData.Dir.Right;
        if (direction == new Vector2Int(0, -1)) return BuildObjData.Dir.Up;
        if (direction == new Vector2Int(1, 0)) return BuildObjData.Dir.Left;
        return BuildObjData.Dir.Down;
    }

    private bool IsValidPosition(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < _gridSize.x && pos.y >= 0 && pos.y < _gridSize.y;
    }

    private GridCell GetNode(Vector2Int position)
    {
        if (_nodeGrid == null) {
            Debug.LogError("Node grid not initialized. Call NavigatePath or use MapData constructor.");
            return null;
        }
        return _nodeGrid[position.x, position.y];
    }
}
