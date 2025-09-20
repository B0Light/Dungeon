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
    
    
    // 웨이포인트 시스템
    private WaypointSystemData _waypointSystem;
    
    // 프로퍼티
    public bool IsMapGenerated => _isMapGenerated;

    protected BaseMapGenerator(Transform slot, TileMappingDataSO tileMappingData)
    {
        this._slot = slot;
        this._tileMappingDataSO = tileMappingData;
        Init();
    }
    
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
    
    protected virtual void InitializeGrid()
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
                if (_grid[x, y] == CellType.FloorCenter || _grid[x, y] == CellType.Floor || _grid[x, y] == CellType.Path || _grid[x, y] == CellType.ExpandedPath)
                {
                    if (_grid[x - 1, y] == CellType.Empty) _grid[x - 1, y] = CellType.Wall;
                    if (_grid[x + 1, y] == CellType.Empty) _grid[x + 1, y] = CellType.Wall;
                    if (_grid[x, y - 1] == CellType.Empty) _grid[x, y - 1] = CellType.Wall;
                    if (_grid[x, y + 1] == CellType.Empty) _grid[x, y + 1] = CellType.Wall;
                }
            }
        }
        
        BuildGate();
    }
    
    private void BuildGate()
    {
        for (int x = 1; x < gridSize.x - 1; x++)
        {
            for (int y = 1; y < gridSize.y - 1; y++)
            {
                // 현재 셀이 통로이거나 확장된 통로일 때만 검사
                if (_grid[x, y] == CellType.Path || _grid[x, y] == CellType.ExpandedPath)
                {
                    bool hasFloorNeighbor = false;
                    bool hasPathNeighbor = false;

                    // 4방향 이웃 탐색
                    int[] dx = { -1, 1, 0, 0 };
                    int[] dy = { 0, 0, -1, 1 };

                    for (int i = 0; i < 4; i++)
                    {
                        int nx = x + dx[i];
                        int ny = y + dy[i];

                        // 그리드 범위 체크
                        if (nx < 0 || nx >= gridSize.x || ny < 0 || ny >= gridSize.y)
                        {
                            continue;
                        }

                        // 이웃 셀 타입 확인
                        if (_grid[nx, ny] == CellType.Floor || _grid[nx, ny] == CellType.FloorCenter)
                        {
                            hasFloorNeighbor = true;
                        }
                        else if (_grid[nx, ny] == CellType.Path || _grid[nx, ny] == CellType.ExpandedPath)
                        {
                            hasPathNeighbor = true;
                        }
                    }

                    // 주변에 Floor와 Path가 모두 존재하고, 현재 셀이 Path일 경우 Gate로 변경
                    if (hasFloorNeighbor && hasPathNeighbor)
                    {
                        _grid[x, y] = CellType.Gate;
                    }
                }
            }
        }
    }
    
    protected virtual void RenderGrid()
    {
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                RenderTileAt(x, y);
            }
        }
    }
    
    protected virtual void RenderTileAt(int x, int y)
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
    
    protected virtual bool TryGetTileData(int x, int y, out TileDataSO tileData)
    {
        return _tileDataDict.TryGetValue(_grid[x, y], out tileData) && tileData != null;
    }
    
    protected virtual void HandleCenterTileRendering(int x, int y, TileDataSO tileData, Vector3 spawnPos)
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
    
    public RoomInfo GetRoomInfo(RectInt room)
    {
        // 정확한 중심 위치 계산 (각 맵 생성기에서 사용하는 방식과 동일)
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
    
    protected virtual void GenerateWaypointSystem()
    {
        if (!_isMapGenerated)
        {
            Debug.LogWarning("맵이 생성되지 않았습니다. 웨이포인트 시스템을 생성할 수 없습니다.");
            return;
        }

        MapData mapData = GetMapData();
        WaypointGenerator waypointGenerator = new WaypointGenerator(mapData, cubeSize);
        _waypointSystem = waypointGenerator.GenerateWaypointSystem();
        
        Debug.Log($"웨이포인트 시스템 생성 완료: {_waypointSystem?.waypoints?.Count ?? 0}개 웨이포인트");
    }
    
    public virtual WaypointSystemData GetWaypointSystemData()
    {
        return _waypointSystem;
    }
    
    public virtual PatrolRoute GetPatrolRoute(string routeName)
    {
        if (_waypointSystem?.patrolRoutes == null) return null;
        
        foreach (var route in _waypointSystem.patrolRoutes)
        {
            if (route.routeName == routeName)
                return route;
        }
        
        return null;
    }
    
    public virtual List<PatrolRoute> GetAllPatrolRoutes()
    {
        return _waypointSystem?.patrolRoutes ?? new List<PatrolRoute>();
    }
    
    public virtual int FindNearestWaypoint(Vector3 worldPosition)
    {
        return _waypointSystem?.FindNearestWaypoint(worldPosition) ?? -1;
    }
    
    protected virtual void InitializeRoomIndices()
    {
        if (_floorList == null || _floorList.Count == 0)
        {
            Debug.LogWarning("방 리스트가 비어있습니다. 방 인덱스를 초기화할 수 없습니다.");
            _startRoomIndex = 0;
            _exitRoomIndex = 0;
            return;
        }

        if (_floorList.Count == 1)
        {
            // 방이 하나뿐인 경우
            _startRoomIndex = 0;
            _exitRoomIndex = 0;
            Debug.LogWarning("방이 하나뿐입니다. 시작점과 출구가 같은 방에 설정됩니다.");
            return;
        }

        // 가장 멀리 떨어진 두 방을 찾기
        FindFurthestRooms();
        
        Debug.Log($"시작 방 인덱스: {_startRoomIndex}, 출구 방 인덱스: {_exitRoomIndex} (총 {_floorList.Count}개 방)");
    }
    
    protected virtual void FindFurthestRooms()
    {
        float maxDistance = 0;
        _startRoomIndex = 0;
        _exitRoomIndex = 0;
        
        for (int i = 0; i < _floorList.Count; i++)
        {
            for (int j = i + 1; j < _floorList.Count; j++)
            {
                Vector2Int centerA = new Vector2Int(
                    _floorList[i].x + (_floorList[i].width - 1) / 2,
                    _floorList[i].y + (_floorList[i].height - 1) / 2
                );
                Vector2Int centerB = new Vector2Int(
                    _floorList[j].x + (_floorList[j].width - 1) / 2,
                    _floorList[j].y + (_floorList[j].height - 1) / 2
                );
                
                float distance = Vector2Int.Distance(centerA, centerB);
                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    _startRoomIndex = i;
                    _exitRoomIndex = j;
                }
            }
        }
    }
    
    protected virtual void OnMapGenerationComplete()
    {
        // 방 인덱스 초기화
        InitializeRoomIndices();
        
        _isMapGenerated = true;
        
        // 웨이포인트 시스템 생성
        GenerateWaypointSystem();
        
        
        Debug.Log($"{GetType().Name}: 맵 생성 완료");
    }
    
    protected virtual void ClearMap()
    {
        if (_slot != null)
        {
            ClearChildrenRecursively(_slot);
        }
    
        if (_grid != null)
        {
            for (int x = 0; x < gridSize.x; x++)
            {
                for (int y = 0; y < gridSize.y; y++)
                {
                    _grid[x, y] = CellType.Empty;
                }
            }
        }
    
        if (_floorList != null)
        {
            _floorList.Clear();
        }
    
        _connectedRoomPairs?.Clear();
        _roomConnections?.Clear();
    
        _isMapGenerated = false;
        Debug.Log($"{GetType().Name}: 맵 제거 완료");
    }
    
    private void ClearChildrenRecursively(Transform parent)
    {
        if (parent == null) return;
        
        // 런타임에서는 즉시 제거를 위해 다른 방법 사용
        if (Application.isPlaying)
        {
            // 런타임에서는 모든 자식을 리스트에 담고 한번에 제거
            var childrenToDestroy = new List<GameObject>();
            for (int i = 0; i < parent.childCount; i++)
            {
                childrenToDestroy.Add(parent.GetChild(i).gameObject);
            }
            
            foreach (var child in childrenToDestroy)
            {
                if (child != null)
                {
                    child.SetActive(false); // 즉시 비활성화
                    UnityEngine.Object.Destroy(child);
                }
            }
        }
        else
        {
            // 에디터에서는 기존 방식 사용
            while (parent.childCount > 0)
            {
                Transform child = parent.GetChild(0);
                if (child != null)
                {
                    UnityEngine.Object.DestroyImmediate(child.gameObject);
                }
            }
        }
    }
    
    public bool HasGeneratedMap()
    {
        return _isMapGenerated && _slot != null && _slot.childCount > 0;
    }
    
    public Vector2Int GetStartPos()
    {
        if (_floorList == null || _floorList.Count == 0)
        {
            Debug.LogError("방 리스트가 비어있습니다. 기본 위치 (0, 0)을 반환합니다.");
            return Vector2Int.zero;
        }
        
        if (_startRoomIndex < 0 || _startRoomIndex >= _floorList.Count)
        {
            Debug.LogError($"시작 방 인덱스가 유효하지 않습니다. 인덱스: {_startRoomIndex}, 방 개수: {_floorList.Count}");
            _startRoomIndex = 0; // 안전한 기본값 설정
        }
        
        // 정확한 중심 위치 계산 (각 맵 생성기에서 사용하는 방식과 동일)
        RectInt startRoom = _floorList[_startRoomIndex];
        return new Vector2Int(
            startRoom.x + (startRoom.width - 1) / 2,
            startRoom.y + (startRoom.height - 1) / 2
        );
    }
    
    public Vector2Int GetExitPos()
    {
        if (_floorList == null || _floorList.Count == 0)
        {
            Debug.LogError("방 리스트가 비어있습니다. 기본 위치 (0, 0)을 반환합니다.");
            return Vector2Int.zero;
        }
        
        if (_exitRoomIndex < 0 || _exitRoomIndex >= _floorList.Count)
        {
            Debug.LogError($"출구 방 인덱스가 유효하지 않습니다. 인덱스: {_exitRoomIndex}, 방 개수: {_floorList.Count}");
            _exitRoomIndex = 0; // 안전한 기본값 설정
        }
        
        // 정확한 중심 위치 계산 (각 맵 생성기에서 사용하는 방식과 동일)
        RectInt exitRoom = _floorList[_exitRoomIndex];
        return new Vector2Int(
            exitRoom.x + (exitRoom.width - 1) / 2,
            exitRoom.y + (exitRoom.height - 1) / 2
        );
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
            (a, b) => 
            { 
                var cost = Vector2Int.Distance(b.Position, endPos);
                var traversalCost = _grid[b.Position.x, b.Position.y] switch
                {
                    CellType.Floor => 10f,
                    CellType.Empty => 5f,
                    CellType.Path => 1f,
                    _ => 1f
                };

                return new DungeonPathfinder2D.PathCost
                {
                    traversable = true,
                    cost = cost + traversalCost
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
        
        Debug.Log($"방 {startRoomIndex} -> {endRoomIndex} 연결 생성");
    }
}
