using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using bkTools;
using Random = UnityEngine.Random;

public enum PathType
{
    AStar,
    Straight,
}

public abstract class BaseMapGenerator : IMapGenerator
{
    protected Vector2Int gridSize = new Vector2Int(64, 64);
    protected Vector3 cubeSize = new Vector3(2, 2, 2);
    protected int margin = 3;
    
    public PathType pathType = PathType.Straight;
    private TileMappingDataSO _tileMappingDataSO;
    private Dictionary<CellType, TileDataSO> _tileDataDict;
    
    private HashSet<(int, int)> _connectedRoomPairs = new HashSet<(int, int)>();
    private Dictionary<int, HashSet<int>> _roomConnections = new Dictionary<int, HashSet<int>>();
    
    [Header("상태")]
    private bool _isMapGenerated = false;
    
    // 공통 그리드 데이터
    protected CellType[,] _grid;
    protected List<RectInt> _floorList;
    private int _startRoomIndex;
    private int _exitRoomIndex;

    private readonly Transform _slot;
    
    
    // 프로퍼티
    public bool IsMapGenerated => _isMapGenerated;
    
    protected BaseMapGenerator(Transform slot, TileMappingDataSO tileMappingData, Vector2Int gridSize, Vector3 cubeSize)
    {
        this._slot = slot;
        this._tileMappingDataSO = tileMappingData;
        this.gridSize = gridSize;
        this.cubeSize = cubeSize;
        Init();
    }

    private void Init()
    {
        BuildTileDataDictionary();
        InitializeGenerator();
    }
    
    protected virtual void InitializeGenerator() { }
    
    private void BuildTileDataDictionary()
    {
        _tileDataDict = new Dictionary<CellType, TileDataSO>();
        if (_tileMappingDataSO == null) return;
        
        foreach (var mapping in _tileMappingDataSO.tileMappings)
        {
            if (!_tileDataDict.ContainsKey(mapping.cellType) && mapping.tileData != null)
                _tileDataDict.Add(mapping.cellType, mapping.tileData);
        }
    }
    
    public abstract void GenerateMap(int seed);
    
    protected void InitializeGrid()
    {
        _grid = new CellType[gridSize.x, gridSize.y];
        _floorList = new List<RectInt>();
        _connectedRoomPairs = new HashSet<(int, int)>();
        _roomConnections = new Dictionary<int, HashSet<int>>(); 
    
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                _grid[x, y] = CellType.Empty;
            }
        }
    }
    
    protected void ExpandPath()
    {
        for (var x = 1; x < gridSize.x - 1; x++)
        {
            for (var y = 1; y < gridSize.y - 1; y++)
            {
                if (_grid[x, y] == CellType.Path)
                {
                    if (_grid[x - 1, y] == CellType.Empty) _grid[x - 1, y] = CellType.ExpandedPath;
                    if (_grid[x + 1, y] == CellType.Empty) _grid[x + 1, y] = CellType.ExpandedPath;
                    if (_grid[x, y - 1] == CellType.Empty) _grid[x, y - 1] = CellType.ExpandedPath;
                    if (_grid[x, y + 1] == CellType.Empty) _grid[x, y + 1] = CellType.ExpandedPath;
                }
            }
        }
    }
    
    protected void BuildWalls()
    {
        for (int x = 1; x < gridSize.x - 1; x++)
        {
            for (int y = 1; y < gridSize.y - 1; y++)
            {
                if (_grid[x, y] != CellType.FloorCenter && _grid[x, y] != CellType.Floor && _grid[x, y] != CellType.Wall)
                {
                    if (_grid[x - 1, y] == CellType.Floor) _grid[x - 1, y] = CellType.Wall;
                    if (_grid[x + 1, y] == CellType.Floor) _grid[x + 1, y] = CellType.Wall;
                    if (_grid[x, y - 1] == CellType.Floor) _grid[x, y - 1] = CellType.Wall;
                    if (_grid[x, y + 1] == CellType.Floor) _grid[x, y + 1] = CellType.Wall;
                }

                if (_grid[x, y] == CellType.Path || _grid[x, y] == CellType.ExpandedPath)
                {
                    if (_grid[x - 1, y] == CellType.Empty) _grid[x - 1, y] = CellType.PathWall;
                    if (_grid[x + 1, y] == CellType.Empty) _grid[x + 1, y] = CellType.PathWall;
                    if (_grid[x, y - 1] == CellType.Empty) _grid[x, y - 1] = CellType.PathWall;
                    if (_grid[x, y + 1] == CellType.Empty) _grid[x, y + 1] = CellType.PathWall;
                }
            }
        }
        
        BuildGate();
    }
    
    private void BuildGate()
    {
        // 기본 Gate 생성
        ProcessGrid((x, y) => _grid[x, y] == CellType.Wall && HasNeighbors(x, y, 
            new[] { CellType.Floor, CellType.FloorCenter }, 
            new[] { CellType.Path }),
            CellType.Gate);
        
        // Gate 주변 확장
        var existingGates = FindCells(CellType.Gate);
        foreach (var gate in existingGates)
        {
            foreach (var neighbor in GetNeighbors(gate))
            {
                if (IsValid(neighbor) && _grid[neighbor.x, neighbor.y] == CellType.Wall && 
                    HasNeighbors(neighbor.x, neighbor.y, 
                        new[] { CellType.Floor, CellType.FloorCenter }, 
                        new[] { CellType.ExpandedPath }))
                {
                    _grid[neighbor.x, neighbor.y] = CellType.Gate;
                }
            }
        }
    }

    private void ProcessGrid(Func<int, int, bool> condition, CellType newType)
    {
        for (int x = 1; x < gridSize.x - 1; x++)
            for (int y = 1; y < gridSize.y - 1; y++)
                if (condition(x, y))
                    _grid[x, y] = newType;
    }

    private List<Vector2Int> FindCells(CellType type)
    {
        var result = new List<Vector2Int>();
        for (int x = 0; x < gridSize.x; x++)
            for (int y = 0; y < gridSize.y; y++)
                if (_grid[x, y] == type)
                    result.Add(new Vector2Int(x, y));
        return result;
    }

    private Vector2Int[] GetNeighbors(Vector2Int pos) => new[]
    {
        pos + Vector2Int.left, pos + Vector2Int.right,
        pos + Vector2Int.down, pos + Vector2Int.up
    };

    private bool IsValid(Vector2Int pos) => 
        pos.x >= 0 && pos.x < gridSize.x && pos.y >= 0 && pos.y < gridSize.y;

    private bool HasNeighbors(int x, int y, CellType[] type1, CellType[] type2)
    {
        var set1 = new HashSet<CellType>(type1);
        var set2 = new HashSet<CellType>(type2);
        bool has1 = false, has2 = false;
        
        foreach (var neighbor in GetNeighbors(new Vector2Int(x, y)))
        {
            if (!IsValid(neighbor)) continue;
            var cell = _grid[neighbor.x, neighbor.y];
            if (set1.Contains(cell)) has1 = true;
            if (set2.Contains(cell)) has2 = true;
            if (has1 && has2) return true;
        }
        return false;
    }
    
    protected void RenderGrid()
    {
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                RenderTileAt(x, y);
            }
        }
    }
    
    private void RenderTileAt(int x, int y)
    {
        if (!TryGetTileData(x, y, out TileDataSO tileData)) return;
        
        Vector3 spawnPos = new Vector3(x * cubeSize.x, 0, y * cubeSize.z);
        if (_grid[x, y] == CellType.FloorCenter)
        {
            HandleCenterTileRendering(x, y, tileData, spawnPos);
        }
        else
        {
            tileData.SpawnTile(spawnPos, cubeSize, _slot);
        }
    }
    
    private bool TryGetTileData(int x, int y, out TileDataSO tileData)
    {
        return _tileDataDict.TryGetValue(_grid[x, y], out tileData) && tileData != null;
    }
    
    private void HandleCenterTileRendering(int x, int y, TileDataSO tileData, Vector3 spawnPos)
    {
        RectInt? room = GetRoomByCenterPosition(x, y);
        if (!room.HasValue)
        {
            Debug.LogWarning( "POS :" + x + ", " + y+ " : NO ROOM");
            tileData.SpawnTile(spawnPos, cubeSize, _slot);
            return;
        }

        if ((_startRoomIndex >= 0 && _startRoomIndex < _floorList.Count && _floorList[_startRoomIndex] == room) || 
            (_exitRoomIndex >= 0 && _exitRoomIndex < _floorList.Count && _floorList[_exitRoomIndex] == room))
        {
            Debug.LogWarning("Room SpawnPoint");
            tileData.SpawnTile(spawnPos, cubeSize, _slot, true);
        }
        else
        {
            RoomInfo roomInfo = GetRoomInfo(room.Value);
            tileData.SpawnTileWithSizeAwareProps(spawnPos, cubeSize, _slot, roomInfo);
        }
    }
    
    private RectInt? GetRoomByCenterPosition(int x, int y)
    {
        if (_grid[x, y] != CellType.FloorCenter)
            return null;

        foreach (var room in _floorList)
        {
            Vector2Int center = new Vector2Int(
                room.x + (room.width - 1) / 2,
                room.y + (room.height - 1) / 2
            );
            
            if (center.x == x && center.y == y)
            {
                return room;
            }
        }

        for (int i = 0; i < _floorList.Count; i++)
        {
            var room = _floorList[i];
            Vector2Int center = new Vector2Int(
                room.x + (room.width - 1) / 2,
                room.y + (room.height - 1) / 2
            );
        }
        return null;
    }
    
    private RoomInfo GetRoomInfo(RectInt room)
    {
        Vector2Int center = new Vector2Int(
            room.x + (room.width - 1) / 2,
            room.y + (room.height - 1) / 2
        );
        
        return new RoomInfo
        {
            position = room.position,
            size = room.size,
            center = center,
            worldPosition = ConvertGridPos(room.position),
            worldCenter = ConvertGridPos(center)
        };
    }
    
    private Vector3 ConvertGridPos(Vector2Int pos)
    {
        Vector3 position = new Vector3(pos.x * cubeSize.x, 0, pos.y * cubeSize.z); // 큐브 크기를 고려한 위치 조정
        return position;
    }
    
    public virtual MapData GetMapData()
    {
        return new MapData
        {
            grid = _grid,
            floorList = _floorList,
            gridSize = gridSize,
            isGenerated = _isMapGenerated
        };
    }
    

    #region Path Connect Method
    
    protected void CreatePathByTriangulate()
    {
        if (_floorList.Count < 3) return;
        
        List<Vertex> vertices = new List<Vertex>();
        
        vertices.AddRange(_floorList.Select(floor => 
            new Vertex<RectInt>(floor.position + ((Vector2)floor.size) / 2, floor)));

        CreateDelaunayPaths(Delaunay2D.Triangulate(vertices));
    }
    
    private void CreateDelaunayPaths(Delaunay2D delaunay)
    {
        var edges = delaunay.Edges.Select(edge => new Kruskal.Edge(edge.U, edge.V)).ToList();
        var vertices = delaunay.Vertices;
        var selectedEdges = new HashSet<Kruskal.Edge>(Kruskal.GetMinimumSpanningTree(edges, vertices));

        // 일부 랜덤한 엣지를 추가하여 더 많은 복도 생성
        foreach (var edge in edges.Where(e => !selectedEdges.Contains(e))) 
        {
            if (Random.value < 0.3) // 확률을 낮춰서 과도한 연결 방지
            {
                selectedEdges.Add(edge);
            }
        }
    
        // 연결 관계 초기화
        _connectedRoomPairs.Clear();
        _roomConnections.Clear();
    
        // 방 인덱스 초기화
        for (int i = 0; i < _floorList.Count; i++)
        {
            _roomConnections[i] = new HashSet<int>();
        }
    
        // 선택된 엣지들로 경로 생성 (중복 및 불필요한 연결 제거)
        foreach (var edge in selectedEdges) 
        {
            var startRoom = (edge.U as Vertex<RectInt>)?.Item;
            var endRoom = (edge.V as Vertex<RectInt>)?.Item;

            if (startRoom == null || endRoom == null) continue;

            // 두 방의 인덱스를 찾기
            int startRoomIndex = FindRoomIndex(startRoom.Value);
            int endRoomIndex = FindRoomIndex(endRoom.Value);
        
            if (startRoomIndex == -1 || endRoomIndex == -1) continue;
        
            // 이미 직접 연결된 방들인지 체크
            if (IsDirectlyConnected(startRoomIndex, endRoomIndex))
            {
                Debug.Log($"방 {startRoomIndex}과 {endRoomIndex}는 이미 직접 연결됨 - 스킵");
                continue;
            }
        
            // 간접 연결 가능한지 체크 (경로가 2개 이하인 경우만)
            if (CanReachIndirectly(startRoomIndex, endRoomIndex, 2))
            {
                Debug.Log($"방 {startRoomIndex}과 {endRoomIndex}는 간접적으로 연결 가능 - 스킵");
                continue;
            }

            // 연결 생성
            CreateConnectionBetweenRooms(startRoomIndex, endRoomIndex, startRoom.Value, endRoom.Value);
        }
    
        Debug.Log($"총 {_connectedRoomPairs.Count}개의 방 연결이 생성됨");
    }

    #endregion
    
    #region Path Generation Methods
    
    protected void CreatePathBetweenPoints(Vector2Int startPos, Vector2Int endPos)
    {
        switch (pathType)
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
        DungeonPathfinder2D aStar = new DungeonPathfinder2D(gridSize);
        
        var path = aStar.FindPath(
            startPos, 
            endPos, 
            (pathNode, currentNode) => 
            { 
                var baseCost = Vector2Int.Distance(currentNode.Position, endPos);
                var traversalCost = _grid[currentNode.Position.x, currentNode.Position.y] switch
                {
                    CellType.Floor => 50f,
                    CellType.Empty => 5f,
                    CellType.Path => 1f,
                    _ => 10f
                };
                
                float floorNeighborPenalty = CalculateFloorNeighborPenalty(currentNode.Position);
                float directionCost = CalculateDirectionChangeCost(pathNode, currentNode, endPos);
                

                return new DungeonPathfinder2D.PathCost
                {
                    traversable = true,
                    cost = baseCost + traversalCost + directionCost + floorNeighborPenalty
                };
            });

        if (path == null) return;

        foreach (var pos in path) 
        {
            if (_grid[pos.x, pos.y] == CellType.Empty) 
            {
                _grid[pos.x, pos.y] = CellType.Path;
            }
        }
    }
    
    private float CalculateFloorNeighborPenalty(Vector2Int position)
    {
        float floorPenalty = 20f; // Floor 인접 시 추가 비용
        float totalPenalty = 0f;
    
        // 8방향 주변 노드 체크
        int[] dx = { -1, -1, -1, 0, 0, 1, 1, 1 };
        int[] dy = { -1, 0, 1, -1, 1, -1, 0, 1 };
    
        for (int i = 0; i < 8; i++)
        {
            int nx = position.x + dx[i];
            int ny = position.y + dy[i];
        
            // 그리드 범위 체크
            if (nx >= 0 && nx < gridSize.x && ny >= 0 && ny < gridSize.y)
            {
                if (_grid[nx, ny] == CellType.Floor || _grid[nx, ny] == CellType.FloorCenter)
                {
                    // 대각선 방향은 조금 더 적은 페널티 적용
                    float penalty = (i % 2 == 0) ? floorPenalty * 0.7f : floorPenalty;
                    totalPenalty += penalty;
                }
            }
        }
    
        return totalPenalty;
    }
    
    private float CalculateDirectionChangeCost(DungeonPathfinder2D.Node pathNode, DungeonPathfinder2D.Node currentNode, Vector2Int endPos)
    {
        if (pathNode == null) return 0f;
    
        // 방향 변경 패널티
        float directionChangePenalty = 15f;
    
        
        Vector2Int previousDirection = GetDirection(pathNode.Position, currentNode.Position);
        Vector2Int currentDirection = GetDirection(currentNode.Position, endPos);
                
        if (previousDirection != currentDirection)
        {
            if (Vector2.Dot(previousDirection, currentDirection) == 0)
            {
                return directionChangePenalty;
            }
        }
        
        return 0f; 
    }

    // 두 위치 사이의 방향을 계산하는 헬퍼 메서드
    private Vector2Int GetDirection(Vector2Int from, Vector2Int to)
    {
        Vector2Int diff = to - from;
        
        // 정규화된 방향 벡터 반환 (8방향)
        return new Vector2Int(
            diff.x == 0 ? 0 : (diff.x > 0 ? 1 : -1),
            diff.y == 0 ? 0 : (diff.y > 0 ? 1 : -1)
        );
}

    private void CreateStraightPath(Vector2Int startPos, Vector2Int endPos)
    {
        Vector2Int direction = new Vector2Int(
            endPos.x > startPos.x ? 1 : endPos.x < startPos.x ? -1 : 0,
            endPos.y > startPos.y ? 1 : endPos.y < startPos.y ? -1 : 0
        );
    
        Vector2Int current = startPos;
    
        while (current != endPos)
        {
            if (current.x >= 0 && current.x < gridSize.x && 
                current.y >= 0 && current.y < gridSize.y)
            {
                if (_grid[current.x, current.y] == CellType.Empty)
                {
                    _grid[current.x, current.y] = CellType.Path;
                }
            }
        
            if (current.x != endPos.x)
            {
                current.x += direction.x;
            }
            else if (current.y != endPos.y)
            {
                current.y += direction.y;
            }
        }
    }
    
    #endregion
    
    private int FindRoomIndex(RectInt room)
    {
        for (int i = 0; i < _floorList.Count; i++)
        {
            if (_floorList[i].Equals(room))
                return i;
        }
        return -1;
    }

    private bool IsDirectlyConnected(int roomA, int roomB)
    {
        var roomPair = roomA < roomB ? (roomA, roomB) : (roomB, roomA);
        return _connectedRoomPairs.Contains(roomPair);
    }

    private bool CanReachIndirectly(int startRoom, int endRoom, int maxDepth)
    {
        if (maxDepth <= 0) return false;
        
        HashSet<int> visited = new HashSet<int>();
        Queue<(int room, int depth)> queue = new Queue<(int, int)>();
        
        queue.Enqueue((startRoom, 0));
        visited.Add(startRoom);
        
        while (queue.Count > 0)
        {
            var (currentRoom, depth) = queue.Dequeue();
            
            if (currentRoom == endRoom && depth > 0)
                return true;
                
            if (depth >= maxDepth) continue;
            
            if (_roomConnections.ContainsKey(currentRoom))
            {
                foreach (int connectedRoom in _roomConnections[currentRoom])
                {
                    if (!visited.Contains(connectedRoom))
                    {
                        visited.Add(connectedRoom);
                        queue.Enqueue((connectedRoom, depth + 1));
                    }
                }
            }
        }
        
        return false;
    }

    private void CreateConnectionBetweenRooms(int startRoomIndex, int endRoomIndex, RectInt startRoom, RectInt endRoom)
    {
        // 연결 정보 저장
        var roomPair = startRoomIndex < endRoomIndex ? 
            (startRoomIndex, endRoomIndex) : (endRoomIndex, startRoomIndex);
        _connectedRoomPairs.Add(roomPair);
        
        // 양방향 연결 정보 업데이트
        _roomConnections[startRoomIndex].Add(endRoomIndex);
        _roomConnections[endRoomIndex].Add(startRoomIndex);

        // 실제 경로 생성
        var startPos = new Vector2Int(
            startRoom.x + (startRoom.width - 1) / 2,
            startRoom.y + (startRoom.height - 1) / 2
        );
        var endPos = new Vector2Int(
            endRoom.x + (endRoom.width - 1) / 2,
            endRoom.y + (endRoom.height - 1) / 2
        );

        CreatePathBetweenPoints(startPos, endPos);
    }
}
