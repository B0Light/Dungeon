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

public abstract class BaseMapGenerator : IMapGenerator
{
    #region Constants
    private static class PathfindingConstants
    {
        public const float DELAUNAY_EDGE_THRESHOLD = 30f;
        public const int MAX_INDIRECT_CONNECTION_DEPTH = 2;
    }

    private static readonly Vector2Int[] CARDINAL_DIRECTIONS = 
    {
        Vector2Int.left, Vector2Int.right, Vector2Int.down, Vector2Int.up
    };

    private readonly int[] _dx = { -1, 1, 0, 0 };
    private readonly int[] _dy = { 0, 0, -1, 1 };
    #endregion

    #region Fields
    protected readonly MapGenerationConfig _config;
    private readonly Transform _slot;
    
    // Grid and room data
    protected FixedGridXZ<GridCell> _fixedGrid;
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
    public Dictionary<int, HashSet<int>> GetRoomConnection => _roomConnections;
    #endregion

    #region Constructor and Initialization
    protected BaseMapGenerator(Transform slot, DungeonGenerateDataSO dungeonGenerateDataSo)
    {
        _slot = slot;
        _config = new MapGenerationConfig(dungeonGenerateDataSo);
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
    public abstract FixedGridXZ<GridCell> GenerateMap(int seed);
    #endregion

    #region Grid Management
    protected void InitializeGrid()
    {
        _fixedGrid = new FixedGridXZ<GridCell>(
            _config.GridSize.x,
            _config.GridSize.y, 
            _config.CubeSize.x,
            _slot.position,
            (x, z) => new GridCell(x, z, CellType.Empty)
        );
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
            for (var z = 1; z < _config.GridSize.y - 1; z++)
            {
                if (_fixedGrid.GetGridObject(x,z).CellType == targetType)
                {
                    positions.Add(new Vector2Int(x, z));
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
            if (IsValidPosition(neighborPos) && _fixedGrid.GetGridObject(neighborPos.x, neighborPos.y).CellType == CellType.Empty)
            {
                _fixedGrid.GetGridObject(neighborPos.x, neighborPos.y).CellType = CellType.ExpandedPath;
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
                if (_fixedGrid.GetGridObject(x, y).CellType == centerType && HasNeighborOfType(x, y, neighborType))
                {
                    _fixedGrid.GetGridObject(x, y).CellType = wallType;
                }
            }
        }
    }

    private bool HasNeighborOfType(int x, int y, CellType targetType)
    {
        for (int i = 0; i < _dx.Length; i++)
        {
            int nx = x + _dx[i];
            int ny = y + _dy[i];
            if (_fixedGrid.GetGridObject(nx, ny).CellType == targetType)
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
            _fixedGrid.GetGridObject(pos.x, pos.y).CellType = CellType.SubGate;
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
        if (_fixedGrid.GetGridObject(pos.x, pos.y).CellType != CellType.Wall) return false;

        var neighbors = GetNeighborCellTypes(pos);
        
        bool hasFloorOrWall = false;
        bool hasPath = false;
        bool hasMainGate = false;
        bool hasWall = false;
        bool hasEmpty = false;

        foreach (var cell in neighbors)
        {
            if (IsFloorOrWallType(cell))
            {
                hasFloorOrWall = true;
            }
            if (IsPathType(cell))
            {
                hasPath = true;
            }
            if (cell == CellType.MainGate)
            {
                hasMainGate = true;
            }
            if (IsWallType(cell))
            {
                hasWall = true;
            }
            if (cell == CellType.Empty)
            {
                hasEmpty = true;
            }
        }

        return hasFloorOrWall && hasPath && hasMainGate && hasWall && !hasEmpty;
    }

    private IEnumerable<CellType> GetNeighborCellTypes(Vector2Int pos)
    {
        return CARDINAL_DIRECTIONS
            .Select(dir => pos + dir)
            .Where(IsValidPosition)
            .Select(p => _fixedGrid.GetGridObject(p.x, p.y).CellType);
    }

    private static bool IsFloorOrWallType(CellType cellType) =>
        cellType == CellType.Floor || cellType == CellType.Wall;

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
        if (!TryGetTileData(_fixedGrid.GetGridObject(x, y).CellType, out var tileData)) return;
        
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
        _mapData ??= new MapData(_fixedGrid, _floorList, _config, _connectedRoomPairs.Count);
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
        var pathfinder = new GridPathfinder(_fixedGrid);
        var path = pathfinder.NavigatePath(startPos, endPos);
        if (path != null)
        {
            Debug.Log($"Path Created : {path.Count}");
            BuildPath(path);
        }
        else
        {
            Debug.LogWarning($"Path Created Fail");
        }
    }
    
    private void BuildPath(List<GridCell> path)
    {
        foreach (var pos in path)
        {
            ProcessPathPosition(pos);
        }
    }

    private void ProcessPathPosition(GridCell pos)
    {
        switch (_fixedGrid.GetGridObject(pos.Position.x, pos.Position.y).CellType)
        {
            case CellType.Empty:
                _fixedGrid.GetGridObject(pos.Position.x, pos.Position.y).CellType = CellType.Path;
                break;
            case CellType.Wall when !IsAdjacentToMainGate(pos.Position):
                _fixedGrid.GetGridObject(pos.Position.x, pos.Position.y).CellType = CellType.MainGate;
                break;
        }
    }

    private bool IsAdjacentToMainGate(Vector2Int pos)
    {
        return CARDINAL_DIRECTIONS
            .Select(dir => pos + dir)
            .Where(neighbor => IsValidPosition(neighbor))
            .Any(neighbor => _fixedGrid.GetGridObject(neighbor.x, neighbor.y).CellType == CellType.MainGate);
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
        if (IsValidPosition(pos) && _fixedGrid.GetGridObject(pos.x, pos.y).CellType == CellType.MainGate)
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
            if (IsValidPosition(neighbor) && IsPathType(_fixedGrid.GetGridObject(neighbor.x, neighbor.y).CellType))
            {
                return new Vector2Int((int)Mathf.Sign(direction.x), (int)Mathf.Sign(direction.y));
            }
        }
        return Vector2Int.zero;
    }
    #endregion
}