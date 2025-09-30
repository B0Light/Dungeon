using System.Collections.Generic;
using UnityEngine;
using System.Linq; // Added for MapGridPathfinder's LINQ usage

public class GridPathfinder : AStarPathfindingBase<GridCell>
{
    // AStarPathfinding's fields
    private GridXZ<GridCell> _grid;
    private GridCell _goalNode;

    // MapGridPathfinder's fields
    private CellType[,] _cellTypeGrid; // Renamed from _grid in MapGridPathfinder to avoid conflict
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
    public float DiagonalMoveCost { get; set; } = 1.414f; // sqrt(2)
    public float StraightMoveCost { get; set; } = 1.0f;

    // Constructor for MapGridPathfinder-like initialization
    public GridPathfinder(MapData mapData)
    {
        _cellTypeGrid = mapData.grid;
        _gridSize = mapData.mapConfig.GridSize;
        _nodeGrid = new GridCell[_gridSize.x, _gridSize.y];
        InitializeNodeGrid();
    }

    // Default constructor for AStarPathfinding-like initialization
    public GridPathfinder() { }

    public List<GridCell> NavigatePath(Vector2Int start, Vector2Int goal)
    {
        // If initialized with MapData, use _nodeGrid directly.
        // Otherwise, get grid from GridBuildingSystem and initialize _nodeGrid.
        if (_nodeGrid == null)
        {
            _grid = GridBuildingSystem.Instance.GetGrid();
            _gridSize = new Vector2Int(_grid.GetGridWidth(), _grid.GetGridHeight());
            _nodeGrid = new GridCell[_gridSize.x, _gridSize.y];
            // Initialize _nodeGrid based on GridXZ for AStarPathfinding's use case
            foreach (GridCell obj in _grid.GetAllGridObjects())
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
                // If initialized with MapData, create GridCell from _cellTypeGrid
                if (_cellTypeGrid != null)
                {
                    _nodeGrid[x, y] = new GridCell(x, y, _cellTypeGrid[x, y]);
                }
                // If initialized via GridBuildingSystem, GridCell objects already exist and are populated in NavigatePath
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
            Vector2Int neighborPos = node.Position + direction; // Changed to node.Position for consistency
            
            if (IsValidPosition(neighborPos))
            {
                var neighborNode = GetNode(neighborPos);
                if (neighborNode == null) continue; // Safety check

                // AStarPathfinding specific logic
                if (_grid != null) // If using GridBuildingSystem grid
                {
                    GridCell currentNeighborGrid = _grid.GetGridObject(neighborPos.x, neighborPos.y);
                    if (currentNeighborGrid?.GetTileType() == TileType.Road) 
                    {
                        yield return currentNeighborGrid;
                    }
                    
                    if ((currentNeighborGrid?.GetTileType() == TileType.Headquarter || currentNeighborGrid?.GetTileType() == TileType.Attraction || currentNeighborGrid?.GetTileType() == TileType.MajorFacility)
                        && currentNeighborGrid == _goalNode) // Check against _goalNode here
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
                // MapGridPathfinder specific logic (or general walkable check)
                else if (neighborNode.IsWalkable)
                {
                    yield return neighborNode;
                }
            }
        }
    }
    
    protected override float GetDistance(GridCell a, GridCell b)
    {
        // If diagonal movement is allowed, use Euclidean distance with custom costs
        if (AllowDiagonalMovement)
        {
            var distance = Vector2Int.Distance(a.Position, b.Position);
            bool isDiagonal = Mathf.Abs(a.Position.x - b.Position.x) == 1 && 
                              Mathf.Abs(a.Position.y - b.Position.y) == 1;
            return isDiagonal ? DiagonalMoveCost : StraightMoveCost; // Return fixed cost for immediate neighbors
        }
        // Otherwise, use Manhattan distance
        else
        {
            int distX = Mathf.Abs(a.GetEntrancePosition().x - b.GetEntrancePosition().x);
            int distY = Mathf.Abs(a.GetEntrancePosition().y - b.GetEntrancePosition().y);
            return distX + distY; // 맨해튼 거리
        }
    }

    private static BuildObjData.Dir ConvertToConnectDirection(Vector2Int direction)
    {
        if (direction == new Vector2Int(0, 1)) return BuildObjData.Dir.Down;
        if (direction == new Vector2Int(-1, 0)) return BuildObjData.Dir.Right;
        if (direction == new Vector2Int(0, -1)) return BuildObjData.Dir.Up;
        if (direction == new Vector2Int(1, 0)) return BuildObjData.Dir.Left;
        return BuildObjData.Dir.Down;
    }

    // Helper methods from MapGridPathfinder
    private bool IsValidPosition(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < _gridSize.x && pos.y >= 0 && pos.y < _gridSize.y;
    }

    public GridCell GetNode(Vector2Int position)
    {
        // Ensure _nodeGrid is initialized before access
        if (_nodeGrid == null) {
            Debug.LogError("Node grid not initialized. Call NavigatePath or use MapData constructor.");
            return null;
        }
        return _nodeGrid[position.x, position.y];
    }
    

    public bool IsPositionWalkable(Vector2Int position)
    {
        if (!IsValidPosition(position)) return false;
        return GetNode(position).IsWalkable;
    }

    public CellType GetCellType(Vector2Int position)
    {
        if (!IsValidPosition(position)) return CellType.Empty;
        // If initialized with MapData, use _cellTypeGrid
        if (_cellTypeGrid != null)
        {
            return _cellTypeGrid[position.x, position.y];
        }
        // Otherwise, use _nodeGrid's CellType
        else if (_nodeGrid != null && _nodeGrid[position.x, position.y] != null)
        {
            return _nodeGrid[position.x, position.y].CellType;
        }
        return CellType.Empty;
    }
}
