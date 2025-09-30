
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapGridPathfinder : AStarPathfindingBase<GridCell>
{
    private readonly CellType[,] _grid;
    private readonly Vector2Int _gridSize;
    private readonly GridCell[,] _nodeGrid;
    
    // 이동 방향 정의 (4방향 또는 8방향)
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
    public float DiagonalMoveCost { get; set; } = 1.414f; // sqrt(2)
    public float StraightMoveCost { get; set; } = 1.0f;

    public MapGridPathfinder(MapData mapData)
    {
        _grid = mapData.grid;
        _gridSize = mapData.mapConfig.GridSize;
        _nodeGrid = new GridCell[_gridSize.x, _gridSize.y];
        
        InitializeNodeGrid();
    }

    private void InitializeNodeGrid()
    {
        for (int x = 0; x < _gridSize.x; x++)
        {
            for (int y = 0; y < _gridSize.y; y++)
            {
                _nodeGrid[x, y] = new GridCell(null, x, y, _grid[x, y]);
            }
        }
    }

    // 공개 패스파인딩 메서드
    public List<Vector2Int> FindPath(Vector2Int startPos, Vector2Int goalPos)
    {
        if (!IsValidPosition(startPos) || !IsValidPosition(goalPos))
        {
            Debug.LogWarning($"Invalid positions: Start({startPos}) or Goal({goalPos})");
            return null;
        }

        var startNode = GetNode(startPos);
        var goalNode = GetNode(goalPos);

        if (!startNode.IsWalkable || !goalNode.IsWalkable)
        {
            Debug.LogWarning($"Unwalkable positions: Start({startPos}:{startNode.CellType}) or Goal({goalPos}:{goalNode.CellType})");
            return null;
        }

        ResetNodeCosts();
        startNode.GCost = 0;

        var nodePath = FindPath(startNode, goalNode);
        return nodePath?.Select(node => node.Position).ToList();
    }

    private void ResetNodeCosts()
    {
        for (int x = 0; x < _gridSize.x; x++)
        {
            for (int y = 0; y < _gridSize.y; y++)
            {
                var node = _nodeGrid[x, y];
                node.GCost = float.MaxValue;
                node.HCost = 0;
                node.Parent = null;
            }
        }
    }

    protected override IEnumerable<GridCell> GetNeighbors(GridCell node)
    {
        var directions = AllowDiagonalMovement ? DirectionsDiagonal : DirectionsCardinal;
        
        foreach (var direction in directions)
        {
            var neighborPos = node.Position + direction;
            
            if (IsValidPosition(neighborPos))
            {
                var neighborNode = GetNode(neighborPos);
                if (neighborNode.IsWalkable)
                {
                    yield return neighborNode;
                }
            }
        }
    }

    protected override float GetDistance(GridCell a, GridCell b)
    {
        var distance = Vector2Int.Distance(a.Position, b.Position);
        
        // 대각선 이동인지 확인
        bool isDiagonal = Mathf.Abs(a.Position.x - b.Position.x) == 1 && 
                         Mathf.Abs(a.Position.y - b.Position.y) == 1;
        
        return isDiagonal ? DiagonalMoveCost : StraightMoveCost;
    }

    private bool IsValidPosition(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < _gridSize.x && pos.y >= 0 && pos.y < _gridSize.y;
    }

    private GridCell GetNode(Vector2Int position)
    {
        return _nodeGrid[position.x, position.y];
    }

    // 디버깅용 메서드들
    public void PrintPath(List<Vector2Int> path)
    {
        if (path == null || path.Count == 0)
        {
            Debug.Log("No path found or empty path");
            return;
        }

        Debug.Log($"Path found with {path.Count} nodes:");
        for (int i = 0; i < path.Count; i++)
        {
            var pos = path[i];
            var cellType = _grid[pos.x, pos.y];
            Debug.Log($"  {i}: ({pos.x}, {pos.y}) - {cellType}");
        }
    }

    public bool IsPositionWalkable(Vector2Int position)
    {
        if (!IsValidPosition(position)) return false;
        return GetNode(position).IsWalkable;
    }

    public CellType GetCellType(Vector2Int position)
    {
        if (!IsValidPosition(position)) return CellType.Empty;
        return _grid[position.x, position.y];
    }
}