using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using bkTools;

public enum MapGeneratorType
{
    BSP,            // Binary Space Partitioning
    BSPFull,        // BSP Full (분할된 영역 전체를 방으로 사용)
    Isaac,          // Isaac 스타일 (BFS 방식)
    Delaunay        // Delaunay 삼각분할 + Kruskal
}

public enum PathType
{
    AStar,
    Straight,
}

public abstract class BaseMapGenerator : IMapGenerator
{
    #region Constants
    private static class PathfindingConstants
    {
        public const float PATH_TRAVERSAL_COST = 0.1f;
        public const float FLOOR_TRAVERSAL_COST = 1f;
        public const float EMPTY_TRAVERSAL_COST = 5f;
        public const float WALL_TRAVERSAL_COST = 10f;
        public const float DEFAULT_TRAVERSAL_COST = 4f;
        public const float DIRECTION_CHANGE_PENALTY = 1000f;
        public const float DELAUNAY_EDGE_THRESHOLD = 30f;
        public const int MAX_INDIRECT_CONNECTION_DEPTH = 2;
    }

    private static readonly Vector2Int[] CARDINAL_DIRECTIONS = 
    {
        Vector2Int.left, Vector2Int.right, Vector2Int.down, Vector2Int.up
    };

    private static readonly int[] DX = { -1, 1, 0, 0 };
    private static readonly int[] DY = { 0, 0, -1, 1 };
    #endregion

    #region Fields
    protected readonly MapGenerationConfig _config;
    private readonly Transform _slot;
    
    // Grid and room data
    protected CellType[,] _grid;
    protected List<RectInt> _floorList;
    private Dictionary<CellType, TileDataSO> _tileDataDict;
    
    // Connection tracking
    private readonly Dictionary<RectInt, List<Vector2Int>> _roomGateDirections = new();
    private readonly HashSet<(int, int)> _connectedRoomPairs = new();
    private readonly Dictionary<int, HashSet<int>> _roomConnections = new();
    private MapData _mapData;
    #endregion

    #region Properties
    public bool IsMapGenerated { get; private set; }
    public Dictionary<RectInt, List<Vector2Int>> RoomGateDirections => _roomGateDirections;
    #endregion

    #region Constructor and Initialization
    protected BaseMapGenerator(Transform slot, DungeonDataSO dungeonDataSo)
    {
        _slot = slot;
        _config = new MapGenerationConfig(dungeonDataSo);
        Initialize();
    }

    private void Initialize()
    {
        BuildTileDataDictionary();
        InitializeGenerator();
    }

    protected virtual void InitializeGenerator() { }
    
    private void BuildTileDataDictionary()
    {
        _tileDataDict = _config.TileMappingDataSO?.tileMappings
            .Where(mapping => mapping.tileData != null)
            .ToDictionary(mapping => mapping.cellType, mapping => mapping.tileData);
    }
    #endregion

    #region Abstract Methods
    public abstract void GenerateMap(int seed);
    #endregion

    #region Grid Management
    protected void InitializeGrid()
    {
        _grid = new CellType[_config.GridSize.x, _config.GridSize.y];
        _floorList = new List<RectInt>();
        ClearConnectionData();
    }

    private void ClearConnectionData()
    {
        _connectedRoomPairs.Clear();
        _roomConnections.Clear();
        _roomGateDirections.Clear();
    }

    private bool IsValidPosition(Vector2Int pos) =>
        pos.x >= 0 && pos.x < _config.GridSize.x && pos.y >= 0 && pos.y < _config.GridSize.y;

    private bool IsWithinBounds(int x, int y) =>
        x >= 1 && x < _config.GridSize.x - 1 && y >= 1 && y < _config.GridSize.y - 1;
    #endregion

    #region Path and Wall Building
    protected void ExpandPath()
    {
        var pathPositions = FindCellsOfType(CellType.Path);
        foreach (var pos in pathPositions)
        {
            ExpandPathAtPosition(pos);
        }
    }

    private List<Vector2Int> FindCellsOfType(CellType targetType)
    {
        var positions = new List<Vector2Int>();
        for (var x = 1; x < _config.GridSize.x - 1; x++)
        {
            for (var y = 1; y < _config.GridSize.y - 1; y++)
            {
                if (_grid[x, y] == targetType)
                {
                    positions.Add(new Vector2Int(x, y));
                }
            }
        }
        return positions;
    }

    private void ExpandPathAtPosition(Vector2Int position)
    {
        foreach (var direction in CARDINAL_DIRECTIONS)
        {
            var neighborPos = position + direction;
            if (IsValidPosition(neighborPos) && _grid[neighborPos.x, neighborPos.y] == CellType.Empty)
            {
                _grid[neighborPos.x, neighborPos.y] = CellType.ExpandedPath;
            }
        }
    }

    protected void BuildWalls() => 
        BuildWallsAroundCellType(CellType.Floor, CellType.Empty, CellType.Wall);
    
    protected void BuildPathWalls() => 
        BuildWallsAroundCellType(CellType.Empty, CellType.ExpandedPath, CellType.PathWall);
    
    protected void BuildSubWalls() => 
        BuildWallsAroundCellType(CellType.Empty, CellType.SubGate, CellType.Wall);
    
    private void BuildWallsAroundCellType(CellType centerType, CellType neighborType, CellType wallType)
    {
        for (int x = 1; x < _config.GridSize.x - 1; x++)
        {
            for (int y = 1; y < _config.GridSize.y - 1; y++)
            {
                if (_grid[x, y] == centerType && HasNeighborOfType(x, y, neighborType))
                {
                    _grid[x, y] = wallType;
                }
            }
        }
    }

    private bool HasNeighborOfType(int x, int y, CellType targetType)
    {
        for (int i = 0; i < DX.Length; i++)
        {
            int nx = x + DX[i];
            int ny = y + DY[i];
            if (_grid[nx, ny] == targetType)
            {
                return true;
            }
        }
        return false;
    }
    #endregion

    #region Gate Building
    protected void BuildGate()
    {
        var gatePositions = FindGateCandidatePositions();
        foreach (var pos in gatePositions)
        {
            _grid[pos.x, pos.y] = CellType.SubGate;
        }
    }

    private List<Vector2Int> FindGateCandidatePositions()
    {
        var candidates = new List<Vector2Int>();
        for (int x = 1; x < _config.GridSize.x - 1; x++)
        {
            for (int y = 1; y < _config.GridSize.y - 1; y++)
            {
                var pos = new Vector2Int(x, y);
                if (IsGateCandidate(pos))
                {
                    candidates.Add(pos);
                }
            }
        }
        return candidates;
    }

    private bool IsGateCandidate(Vector2Int pos)
    {
        if (_grid[pos.x, pos.y] != CellType.Wall) return false;

        var neighbors = GetNeighborCellTypes(pos);
        
        return neighbors.Any(IsFloorOrWallType) &&
               neighbors.Any(IsPathType) &&
               neighbors.Any(cell => cell == CellType.MainGate) &&
               neighbors.Any(IsWallType) &&
               neighbors.All(cell => cell != CellType.Empty);
    }

    private IEnumerable<CellType> GetNeighborCellTypes(Vector2Int pos)
    {
        return CARDINAL_DIRECTIONS
            .Select(dir => pos + dir)
            .Where(IsValidPosition)
            .Select(p => _grid[p.x, p.y]);
    }

    private static bool IsFloorOrWallType(CellType cellType) =>
        cellType == CellType.Floor || cellType == CellType.FloorCenter || cellType == CellType.Wall;

    private static bool IsPathType(CellType cellType) =>
        cellType == CellType.ExpandedPath || cellType == CellType.Path;

    private static bool IsWallType(CellType cellType) =>
        cellType == CellType.Wall || cellType == CellType.PathWall;
    #endregion

    #region Rendering
    protected void RenderGrid()
    {
        for (int x = 0; x < _config.GridSize.x; x++)
        {
            for (int y = 0; y < _config.GridSize.y; y++)
            {
                RenderTileAt(x, y);
            }
        }
    }

    private void RenderTileAt(int x, int y)
    {
        if (!TryGetTileData(_grid[x, y], out var tileData)) return;
        
        Vector3 spawnPos = new Vector3(x * _config.CubeSize.x, 0, y * _config.CubeSize.z);
        tileData.SpawnTile(spawnPos, _config.CubeSize, _slot);
    }

    private bool TryGetTileData(CellType cellType, out TileDataSO tileData)
    {
        tileData = null;
        return _tileDataDict?.TryGetValue(cellType, out tileData) == true && tileData != null;
    }
    #endregion

    #region Map Data
    public MapData GetMapData()
    {
        _mapData ??= new MapData(_grid, _floorList, _config, _connectedRoomPairs.Count);
        return _mapData;
    }
    #endregion

    #region Delaunay Triangulation Path Creation
    protected void CreatePathByTriangulate()
    {
        if (_floorList.Count < 3) return;

        var vertices = CreateVerticesFromFloors();
        var delaunay = Delaunay2D.Triangulate(vertices);
        CreateDelaunayPaths(delaunay);
    }

    private List<Vertex> CreateVerticesFromFloors()
    {
        List<Vertex> vertices = new List<Vertex>(); 
        vertices.AddRange(_floorList.Select(floor =>  
            new Vertex<RectInt>(floor.position + ((Vector2)floor.size) / 2, floor)));
       return vertices;
    }

    private void CreateDelaunayPaths(Delaunay2D delaunay)
    {
        var selectedEdges = GetSelectedEdges(delaunay);
        InitializeRoomConnections();
        ProcessSelectedEdges(selectedEdges);
        
        Debug.Log($"총 {_connectedRoomPairs.Count}개의 방 연결이 생성됨");
    }

    private HashSet<Kruskal.Edge> GetSelectedEdges(Delaunay2D delaunay)
    {
        var edges = delaunay.Edges.Select(edge => new Kruskal.Edge(edge.U, edge.V)).ToList();
        var selectedEdges = new HashSet<Kruskal.Edge>(Kruskal.GetMinimumSpanningTree(edges, delaunay.Vertices));

        // Add longer edges for more connectivity
        foreach (var edge in edges.Where(e => !selectedEdges.Contains(e) && 
                                             e.Distance > PathfindingConstants.DELAUNAY_EDGE_THRESHOLD))
        {
            selectedEdges.Add(edge);
        }

        return selectedEdges;
    }

    private void InitializeRoomConnections()
    {
        _connectedRoomPairs.Clear();
        _roomConnections.Clear();

        for (int i = 0; i < _floorList.Count; i++)
        {
            _roomConnections[i] = new HashSet<int>();
        }
    }

    private void ProcessSelectedEdges(HashSet<Kruskal.Edge> selectedEdges)
    {
        foreach (var edge in selectedEdges)
        {
            if (!TryGetRoomsFromEdge(edge, out var startRoom, out var endRoom)) continue;

            int startRoomIndex = _floorList.IndexOf(startRoom);
            int endRoomIndex = _floorList.IndexOf(endRoom);

            if (startRoomIndex == -1 || endRoomIndex == -1) continue;
            if (ShouldSkipConnection(startRoomIndex, endRoomIndex)) continue;

            CreateConnectionBetweenRooms(startRoomIndex, endRoomIndex, startRoom, endRoom);
        }
    }

    private bool TryGetRoomsFromEdge(Kruskal.Edge edge, out RectInt startRoom, out RectInt endRoom)
    {
        startRoom = default;
        endRoom = default;

        if (!(edge.U is Vertex<RectInt> startVertex) || !(edge.V is Vertex<RectInt> endVertex))
            return false;

        startRoom = startVertex.Item;
        endRoom = endVertex.Item;
        return true;
    }

    private bool ShouldSkipConnection(int startRoomIndex, int endRoomIndex)
    {
        return IsDirectlyConnected(startRoomIndex, endRoomIndex) ||
               CanReachIndirectly(startRoomIndex, endRoomIndex, PathfindingConstants.MAX_INDIRECT_CONNECTION_DEPTH);
    }
    #endregion

    #region Room Connection Logic
    private bool IsDirectlyConnected(int roomA, int roomB)
    {
        var roomPair = roomA < roomB ? (roomA, roomB) : (roomB, roomA);
        return _connectedRoomPairs.Contains(roomPair);
    }

    private bool CanReachIndirectly(int startRoom, int endRoom, int maxDepth)
    {
        if (maxDepth <= 0) return false;

        var visited = new HashSet<int>();
        var queue = new Queue<(int room, int depth)>();

        queue.Enqueue((startRoom, 0));
        visited.Add(startRoom);

        while (queue.Count > 0)
        {
            var (currentRoom, depth) = queue.Dequeue();

            if (currentRoom == endRoom && depth > 0)
                return true;

            if (depth >= maxDepth) continue;

            if (_roomConnections.TryGetValue(currentRoom, out var connectedRooms))
            {
                foreach (int connectedRoom in connectedRooms.Where(room => !visited.Contains(room)))
                {
                    visited.Add(connectedRoom);
                    queue.Enqueue((connectedRoom, depth + 1));
                }
            }
        }

        return false;
    }

    private void CreateConnectionBetweenRooms(int startRoomIndex, int endRoomIndex, RectInt startRoom, RectInt endRoom)
    {
        UpdateConnectionTracking(startRoomIndex, endRoomIndex);
        CreatePhysicalPath(startRoom, endRoom);
    }

    private void UpdateConnectionTracking(int startRoomIndex, int endRoomIndex)
    {
        var roomPair = startRoomIndex < endRoomIndex
            ? (startRoomIndex, endRoomIndex)
            : (endRoomIndex, startRoomIndex);
        
        _connectedRoomPairs.Add(roomPair);
        _roomConnections[startRoomIndex].Add(endRoomIndex);
        _roomConnections[endRoomIndex].Add(startRoomIndex);
    }

    private void CreatePhysicalPath(RectInt startRoom, RectInt endRoom)
    {
        var startPos = GetRoomCenter(startRoom);
        var endPos = GetRoomCenter(endRoom);
        CreatePathBetweenPoints(startPos, endPos);
    }

    private Vector2Int GetRoomCenter(RectInt room)
    {
        return new Vector2Int(
            room.x + room.width / 2,
            room.y + room.height / 2
        );
    }
    #endregion

    #region Path Generation
    protected void CreatePathBetweenPoints(Vector2Int startPos, Vector2Int endPos)
    {
        switch (_config.PathType)
        {
            case PathType.AStar:
                CreateAStarPath(startPos, endPos);
                break;
            case PathType.Straight:
                CreateStraightPath(startPos, endPos);
                break;
        }
    }

    private void CreateAStarPath(Vector2Int startPos, Vector2Int endPos)
    {
        var pathfinder = new DungeonPathfinder2D(_config.GridSize);
        var path = pathfinder.FindPath(startPos, endPos, CalculateAStarCost);
        if (path != null) BuildPath(path);
    }

    private void CreateStraightPath(Vector2Int startPos, Vector2Int endPos)
    {
        var pathfinder = new DungeonPathfinder2D(_config.GridSize);
        var path = pathfinder.FindPath(startPos, endPos, CalculateStraightPathCost);
        if (path != null) BuildPath(path);
    }

    private DungeonPathfinder2D.PathCost CalculateAStarCost(
        DungeonPathfinder2D.Node pathNode, 
        DungeonPathfinder2D.Node currentNode)
    {
        var traversalCost = GetTraversalCost(currentNode.Position);
        var directionCost = CalculateDirectionChangeCost(pathNode, currentNode);
        
        return new DungeonPathfinder2D.PathCost
        {
            traversable = true,
            cost = traversalCost + directionCost
        };
    }

    private DungeonPathfinder2D.PathCost CalculateStraightPathCost(
        DungeonPathfinder2D.Node pathNode, 
        DungeonPathfinder2D.Node currentNode)
    {
        // Manhattan distance for straight paths
        var baseCost = Mathf.Abs(currentNode.Position.x) + Mathf.Abs(currentNode.Position.y);
        var directionCost = CalculateDirectionChangeCost(pathNode, currentNode);
        
        return new DungeonPathfinder2D.PathCost
        {
            traversable = true,
            cost = baseCost + directionCost
        };
    }

    private float GetTraversalCost(Vector2Int position)
    {
        return _grid[position.x, position.y] switch
        {
            CellType.Path => PathfindingConstants.PATH_TRAVERSAL_COST,
            CellType.Floor => PathfindingConstants.FLOOR_TRAVERSAL_COST,
            CellType.Empty => PathfindingConstants.EMPTY_TRAVERSAL_COST,
            CellType.Wall => PathfindingConstants.WALL_TRAVERSAL_COST,
            _ => PathfindingConstants.DEFAULT_TRAVERSAL_COST
        };
    }

    private float CalculateDirectionChangeCost(DungeonPathfinder2D.Node pathNode, DungeonPathfinder2D.Node currentNode)
    {
        if (pathNode == null) return 0f;

        var previousDirection = GetDirection(pathNode.Position, currentNode.Position);
        var currentDirection = GetDirection(currentNode.Position, currentNode.Position); // This might need endPos

        return previousDirection != currentDirection ? PathfindingConstants.DIRECTION_CHANGE_PENALTY : 0f;
    }

    private Vector2Int GetDirection(Vector2Int from, Vector2Int to)
    {
        Vector2Int diff = to - from;

        // Prioritize the larger difference
        if (Mathf.Abs(diff.x) > Mathf.Abs(diff.y))
        {
            return new Vector2Int(Math.Sign(diff.x), 0);
        }
        else
        {
            return new Vector2Int(0, Math.Sign(diff.y));
        }
    }

    private void BuildPath(List<Vector2Int> path)
    {
        foreach (var pos in path)
        {
            ProcessPathPosition(pos);
        }
    }

    private void ProcessPathPosition(Vector2Int pos)
    {
        switch (_grid[pos.x, pos.y])
        {
            case CellType.Empty:
                _grid[pos.x, pos.y] = CellType.Path;
                break;
            case CellType.Wall when !IsAdjacentToMainGate(pos):
                _grid[pos.x, pos.y] = CellType.MainGate;
                break;
        }
    }

    private bool IsAdjacentToMainGate(Vector2Int pos)
    {
        return CARDINAL_DIRECTIONS
            .Select(dir => pos + dir)
            .Where(neighbor => IsValidPosition(neighbor))
            .Any(neighbor => _grid[neighbor.x, neighbor.y] == CellType.MainGate);
    }
    #endregion

    #region Gate Direction Management
    protected void PopulateRoomGateDirections()
    {
        _roomGateDirections.Clear();
        Debug.Log($"FloorList Count: {_floorList.Count}");

        foreach (var room in _floorList)
        {
            var gateDirections = FindGateDirectionsForRoom(room);
            if (gateDirections.Any())
            {
                _roomGateDirections[room] = gateDirections;
            }
        }
    }

    private List<Vector2Int> FindGateDirectionsForRoom(RectInt room)
    {
        var gateDirections = new List<Vector2Int>();

        // Check horizontal boundaries (top and bottom)
        for (int x = room.x; x < room.x + room.width; x++)
        {
            CheckForGateDirection(new Vector2Int(x, room.y), gateDirections);
            CheckForGateDirection(new Vector2Int(x, room.y + room.height - 1), gateDirections);
        }

        // Check vertical boundaries (left and right)
        for (int y = room.y; y < room.y + room.height; y++)
        {
            CheckForGateDirection(new Vector2Int(room.x, y), gateDirections);
            CheckForGateDirection(new Vector2Int(room.x + room.width - 1, y), gateDirections);
        }

        return gateDirections;
    }

    private void CheckForGateDirection(Vector2Int pos, List<Vector2Int> gateDirections)
    {
        if (IsValidPosition(pos) && _grid[pos.x, pos.y] == CellType.MainGate)
        {
            var direction = GetGateDirection(pos);
            if (direction != Vector2Int.zero)
            {
                gateDirections.Add(direction);
            }
        }
    }

    private Vector2Int GetGateDirection(Vector2Int gatePos)
    {
        foreach (var direction in CARDINAL_DIRECTIONS)
        {
            var neighbor = gatePos + direction;
            if (IsValidPosition(neighbor) && IsPathType(_grid[neighbor.x, neighbor.y]))
            {
                return new Vector2Int((int)Mathf.Sign(direction.x), (int)Mathf.Sign(direction.y));
            }
        }
        return Vector2Int.zero;
    }
    #endregion
}