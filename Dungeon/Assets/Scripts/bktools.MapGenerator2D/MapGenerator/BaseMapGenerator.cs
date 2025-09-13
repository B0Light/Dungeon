using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using bkTools;

/// <summary>
/// 경로 생성 타입을 정의하는 열거형
/// </summary>
public enum PathType
{
    /// <summary>
    /// A* 알고리즘을 사용한 경로 찾기 (Delaunay 방식)
    /// </summary>
    AStar,
    /// <summary>
    /// L자 형태의 직선 복도 (BSP 방식)
    /// </summary>
    LShaped,
    /// <summary>
    /// 직선 복도 (Isaac 방식)
    /// </summary>
    Straight,
    /// <summary>
    /// 사용자 정의 경로
    /// </summary>
    Custom
}

/// <summary>
/// 맵 생성기의 기본 추상 클래스
/// 공통 기능들을 구현하고 하위 클래스에서 오버라이드할 메서드들을 정의합니다.
/// </summary>
public abstract class BaseMapGenerator : IMapGenerator
{
    [Header("기본 설정")]
    public int seed = 0;
    protected Vector2Int gridSize = new Vector2Int(64, 64);
    protected Vector3 cubeSize = new Vector3(2, 2, 2);
    
    [Header("경로 설정")]
    [SerializeField] public PathType pathType = PathType.AStar;
    [SerializeField, Range(0, 1)] public float pathValue = 0.5f; // A* 경로 생성 확률
    
    [Header("Tile Data Map")]
    protected TileMappingDataSO tileMappingDataSO;
    protected Dictionary<CellType, TileDataSO> tileDataDict;
    
    [Header("상태")]
    [SerializeField] protected bool isMapGenerated = false;
    
    // 공통 그리드 데이터
    protected CellType[,] _grid;
    protected List<RectInt> _floorList;
    protected int _startRoomIndex;
    protected int _exitRoomIndex;

    protected Transform _slot;
    
    // 웨이포인트 시스템
    protected WaypointSystemData _waypointSystem;
    
    // 프로퍼티
    public bool IsMapGenerated => isMapGenerated;

    public BaseMapGenerator(Transform slot, TileMappingDataSO tileMappingData)
    {
        this._slot = slot;
        this.tileMappingDataSO = tileMappingData;
        Init();
    }
    
    public BaseMapGenerator(Transform slot, TileMappingDataSO tileMappingData, Vector2Int gridSize, Vector3 cubeSize)
    {
        this._slot = slot;
        this.tileMappingDataSO = tileMappingData;
        this.gridSize = gridSize;
        this.cubeSize = cubeSize;
        Init();
    }

    private void Init()
    {
        BuildTileDataDictionary();
        InitializeGenerator();
        
        if (seed != 0)
            Random.InitState(seed);
    }
    
    /// <summary>
    /// 생성기 초기화 (하위 클래스에서 구현)
    /// </summary>
    protected virtual void InitializeGenerator() { }
    
    /// <summary>
    /// 타일 데이터 딕셔너리를 빌드합니다.
    /// </summary>
    protected virtual void BuildTileDataDictionary()
    {
        tileDataDict = new Dictionary<CellType, TileDataSO>();
        if (tileMappingDataSO == null) return;
        
        foreach (var mapping in tileMappingDataSO.tileMappings)
        {
            if (!tileDataDict.ContainsKey(mapping.cellType) && mapping.tileData != null)
                tileDataDict.Add(mapping.cellType, mapping.tileData);
        }
    }
    
    /// <summary>
    /// 맵을 생성합니다. (추상 메서드)
    /// </summary>
    public abstract void GenerateMap();
    
    /// <summary>
    /// 그리드를 초기화합니다.
    /// </summary>
    protected virtual void InitializeGrid()
    {
        _grid = new CellType[gridSize.x, gridSize.y];
        _floorList = new List<RectInt>();
        
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                _grid[x, y] = CellType.Empty;
            }
        }
    }
    
    /// <summary>
    /// 길을 확장합니다.
    /// </summary>
    protected void ExpandPath()
    {
        // 벽 배치: 바닥 셀 주변이 빈 셀이라면 그곳에 벽을 배치
        for (var x = 1; x < gridSize.x - 1; x++)
        {
            for (var y = 1; y < gridSize.y - 1; y++)
            {
                if (_grid[x, y] == CellType.Path)
                {
                    // 상하좌우 체크
                    if (_grid[x - 1, y] == CellType.Empty) _grid[x - 1, y] = CellType.ExpandedPath;
                    if (_grid[x + 1, y] == CellType.Empty) _grid[x + 1, y] = CellType.ExpandedPath;
                    if (_grid[x, y - 1] == CellType.Empty) _grid[x, y - 1] = CellType.ExpandedPath;
                    if (_grid[x, y + 1] == CellType.Empty) _grid[x, y + 1] = CellType.ExpandedPath;
                }
            }
        }
    }
    
    /// <summary>
    /// 벽을 생성합니다.
    /// </summary>
    protected void BuildWalls()
    {
        for (int x = 1; x < gridSize.x - 1; x++)
        {
            for (int y = 1; y < gridSize.y - 1; y++)
            {
                if (_grid[x, y] == CellType.Floor || _grid[x, y] == CellType.Path || _grid[x, y] == CellType.ExpandedPath)
                {
                    if (_grid[x - 1, y] == CellType.Empty) _grid[x - 1, y] = CellType.Wall;
                    if (_grid[x + 1, y] == CellType.Empty) _grid[x + 1, y] = CellType.Wall;
                    if (_grid[x, y - 1] == CellType.Empty) _grid[x, y - 1] = CellType.Wall;
                    if (_grid[x, y + 1] == CellType.Empty) _grid[x, y + 1] = CellType.Wall;
                }
            }
        }
    }
    
    /// <summary>
    /// 그리드를 렌더링합니다.
    /// </summary>
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
    
    /// <summary>
    /// 특정 위치의 타일을 렌더링합니다.
    /// </summary>
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
    
    /// <summary>
    /// 타일 데이터를 가져옵니다.
    /// </summary>
    protected virtual bool TryGetTileData(int x, int y, out TileDataSO tileData)
    {
        return tileDataDict.TryGetValue(_grid[x, y], out tileData) && tileData != null;
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
            // 정확한 중심 위치 계산 (각 맵 생성기에서 사용하는 방식과 동일)
            Vector2Int center = new Vector2Int(
                room.x + (room.width - 1) / 2,
                room.y + (room.height - 1) / 2
            );
            
            if (center.x == x && center.y == y)
            {
                Debug.Log($"FIND ROOM! Position: ({x}, {y}), Room: {room}");
                return room;
            }
        }

        Debug.LogWarning($"Room not found at position ({x}, {y}). Available rooms:");
        for (int i = 0; i < _floorList.Count; i++)
        {
            var room = _floorList[i];
            Vector2Int center = new Vector2Int(
                room.x + (room.width - 1) / 2,
                room.y + (room.height - 1) / 2
            );
            Debug.LogWarning($"  Room {i}: {room}, Center: ({center.x}, {center.y})");
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
    
    /// <summary>
    /// 맵 데이터를 가져옵니다.
    /// </summary>
    public virtual MapData GetMapData()
    {
        return new MapData
        {
            grid = _grid,
            floorList = _floorList,
            gridSize = gridSize,
            isGenerated = isMapGenerated
        };
    }
    
    /// <summary>
    /// 웨이포인트 시스템을 생성합니다.
    /// </summary>
    protected virtual void GenerateWaypointSystem()
    {
        if (!isMapGenerated)
        {
            Debug.LogWarning("맵이 생성되지 않았습니다. 웨이포인트 시스템을 생성할 수 없습니다.");
            return;
        }

        MapData mapData = GetMapData();
        WaypointGenerator waypointGenerator = new WaypointGenerator(mapData, cubeSize);
        _waypointSystem = waypointGenerator.GenerateWaypointSystem();
        
        Debug.Log($"웨이포인트 시스템 생성 완료: {_waypointSystem?.waypoints?.Count ?? 0}개 웨이포인트");
    }
    
    /// <summary>
    /// 웨이포인트 시스템 데이터를 가져옵니다.
    /// </summary>
    public virtual WaypointSystemData GetWaypointSystemData()
    {
        return _waypointSystem;
    }
    
    /// <summary>
    /// 특정 패트롤 경로를 가져옵니다.
    /// </summary>
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
    
    /// <summary>
    /// 모든 패트롤 경로를 가져옵니다.
    /// </summary>
    public virtual List<PatrolRoute> GetAllPatrolRoutes()
    {
        return _waypointSystem?.patrolRoutes ?? new List<PatrolRoute>();
    }
    
    /// <summary>
    /// 특정 위치에서 가장 가까운 웨이포인트를 찾습니다.
    /// </summary>
    public virtual int FindNearestWaypoint(Vector3 worldPosition)
    {
        return _waypointSystem?.FindNearestWaypoint(worldPosition) ?? -1;
    }
    
    /// <summary>
    /// 시작 방과 출구 방의 인덱스를 초기화합니다.
    /// </summary>
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
    
    /// <summary>
    /// 가장 멀리 떨어진 두 방을 찾습니다.
    /// </summary>
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

    /// <summary>
    /// 맵 생성 완료를 표시합니다.
    /// </summary>
    protected virtual void OnMapGenerationComplete()
    {
        // 방 인덱스 초기화
        InitializeRoomIndices();
        
        isMapGenerated = true;
        
        // 웨이포인트 시스템 생성
        GenerateWaypointSystem();
        
        
        Debug.Log($"{GetType().Name}: 맵 생성 완료");
    }
    
    /// <summary>
    /// 생성된 맵을 제거합니다.
    /// </summary>
    public virtual void ClearMap()
    {
        if (_slot != null)
        {
            // slot 하위의 모든 자식 오브젝트 제거
            ClearChildrenRecursively(_slot);
        }
        
        // 그리드 초기화
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
        
        // 바닥 리스트 초기화
        if (_floorList != null)
        {
            _floorList.Clear();
        }
        
        isMapGenerated = false;
        Debug.Log($"{GetType().Name}: 맵 제거 완료");
    }
    
    /// <summary>
    /// 자식 오브젝트들을 재귀적으로 제거합니다.
    /// </summary>
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
    
    /// <summary>
    /// 맵이 생성되었는지 확인합니다.
    /// </summary>
    public bool HasGeneratedMap()
    {
        return isMapGenerated && _slot != null && _slot.childCount > 0;
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
    
    #region Path Generation Methods
    
    /// <summary>
    /// 설정된 PathType에 따라 두 점 사이의 경로를 생성합니다.
    /// </summary>
    /// <param name="startPos">시작 위치</param>
    /// <param name="endPos">끝 위치</param>
    protected virtual void CreatePathBetweenPoints(Vector2Int startPos, Vector2Int endPos)
    {
        switch (pathType)
        {
            case PathType.AStar:
                CreateAStarPath(startPos, endPos);
                break;
            case PathType.LShaped:
                CreateLShapedPath(startPos, endPos);
                break;
            case PathType.Straight:
                CreateStraightPath(startPos, endPos);
                break;
            case PathType.Custom:
                CreateCustomPath(startPos, endPos);
                break;
        }
    }
    
    /// <summary>
    /// A* 알고리즘을 사용한 경로 생성 (Delaunay 방식)
    /// </summary>
    /// <param name="startPos">시작 위치</param>
    /// <param name="endPos">끝 위치</param>
    protected virtual void CreateAStarPath(Vector2Int startPos, Vector2Int endPos)
    {
        DungeonPathfinder2D aStar = new DungeonPathfinder2D(gridSize);
        
        var path = aStar.FindPath(
            startPos, 
            endPos, 
            (a, b) => 
            { 
                // 휴리스틱 비용 계산
                var cost = Vector2Int.Distance(b.Position, endPos);

                // 셀 타입에 따른 이동 비용 결정
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

        // 경로에 복도 배치
        foreach (var pos in path) 
        {
            if (_grid[pos.x, pos.y] == CellType.Empty) 
            {
                _grid[pos.x, pos.y] = CellType.Path;
            }
        }
    }
    
    /// <summary>
    /// L자 형태의 복도 생성 (BSP 방식)
    /// </summary>
    /// <param name="startPos">시작 위치</param>
    /// <param name="endPos">끝 위치</param>
    protected virtual void CreateLShapedPath(Vector2Int startPos, Vector2Int endPos)
    {
        // L자 형태로 복도 생성
        if (Random.value < 0.5f)
        {
            // 가로 먼저, 세로 나중
            for (int x = Mathf.Min(startPos.x, endPos.x); x <= Mathf.Max(startPos.x, endPos.x); x++)
            {
                if (x >= 0 && x < gridSize.x && startPos.y >= 0 && startPos.y < gridSize.y)
                    _grid[x, startPos.y] = CellType.Path;
            }
            for (int y = Mathf.Min(startPos.y, endPos.y); y <= Mathf.Max(startPos.y, endPos.y); y++)
            {
                if (endPos.x >= 0 && endPos.x < gridSize.x && y >= 0 && y < gridSize.y)
                    _grid[endPos.x, y] = CellType.Path;
            }
        }
        else
        {
            // 세로 먼저, 가로 나중
            for (int y = Mathf.Min(startPos.y, endPos.y); y <= Mathf.Max(startPos.y, endPos.y); y++)
            {
                if (startPos.x >= 0 && startPos.x < gridSize.x && y >= 0 && y < gridSize.y)
                    _grid[startPos.x, y] = CellType.Path;
            }
            for (int x = Mathf.Min(startPos.x, endPos.x); x <= Mathf.Max(startPos.x, endPos.x); x++)
            {
                if (x >= 0 && x < gridSize.x && endPos.y >= 0 && endPos.y < gridSize.y)
                    _grid[x, endPos.y] = CellType.Path;
            }
        }
    }
    
    /// <summary>
    /// 직선 복도 생성 (Isaac 방식)
    /// </summary>
    /// <param name="startPos">시작 위치</param>
    /// <param name="endPos">끝 위치</param>
    protected virtual void CreateStraightPath(Vector2Int startPos, Vector2Int endPos)
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
            
            // X축 먼저 이동
            if (current.x != endPos.x)
            {
                current.x += direction.x;
            }
            // Y축 이동
            else if (current.y != endPos.y)
            {
                current.y += direction.y;
            }
        }
    }
    
    /// <summary>
    /// 사용자 정의 경로 생성 (하위 클래스에서 오버라이드)
    /// </summary>
    /// <param name="startPos">시작 위치</param>
    /// <param name="endPos">끝 위치</param>
    protected virtual void CreateCustomPath(Vector2Int startPos, Vector2Int endPos)
    {
        // 기본적으로 L자 형태로 생성 (하위 클래스에서 오버라이드 가능)
        CreateLShapedPath(startPos, endPos);
    }
    
    /// <summary>
    /// Delaunay 삼각분할과 Kruskal MST를 사용한 경로 생성
    /// </summary>
    /// <param name="delaunay">Delaunay 삼각분할 결과</param>
    /// <param name="pathValue">추가 경로 생성 확률</param>
    protected virtual void CreateDelaunayPaths(Delaunay2D delaunay, float pathValue = 0.5f)
    {
        var edges = delaunay.Edges.Select(edge => new Kruskal.Edge(edge.U, edge.V)).ToList();
        var vertices = delaunay.Vertices;
        var selectedEdges = new HashSet<Kruskal.Edge>(Kruskal.GetMinimumSpanningTree(edges, vertices));

        // 일부 랜덤한 엣지를 추가하여 더 많은 복도 생성
        foreach (var edge in edges.Where(e => !selectedEdges.Contains(e))) 
        {
            if (Random.value < pathValue) 
            {
                selectedEdges.Add(edge);
            }
        }
        
        // 선택된 엣지들로 경로 생성
        foreach (var edge in selectedEdges) 
        {
            var startRoom = (edge.U as Vertex<RectInt>)?.Item;
            var endRoom = (edge.V as Vertex<RectInt>)?.Item;

            if (startRoom == null || endRoom == null) continue;

            var startPos = new Vector2Int(
                (int)startRoom?.center.x, 
                (int)startRoom?.center.y
            );
            var endPos = new Vector2Int(
                (int)endRoom?.center.x, 
                (int)endRoom?.center.y
            );

            CreatePathBetweenPoints(startPos, endPos);
        }
    }
    
    #endregion
}
