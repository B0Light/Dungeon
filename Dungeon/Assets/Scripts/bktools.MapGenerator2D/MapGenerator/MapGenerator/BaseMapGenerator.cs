using System;
 using System.Collections.Generic;
 using System.Linq;
 using UnityEngine;
 using bkTools;
 using Random = UnityEngine.Random;
 
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
     // Configurable properties from ScriptableObject, now using a single data class
     protected readonly MapGenerationConfig _config;
     private readonly Transform _slot;
 
     // Runtime state
     private bool _isMapGenerated = false;
     
     // Common grid data
     protected CellType[,] _grid;
     protected List<RectInt> _floorList;
     private Dictionary<CellType, TileDataSO> _tileDataDict;
     
     // Room connection tracking
     private readonly HashSet<(int, int)> _connectedRoomPairs = new();
     private readonly Dictionary<int, HashSet<int>> _roomConnections = new();
 
     // Public property
     public bool IsMapGenerated => _isMapGenerated;
     
     // Constructor
     protected BaseMapGenerator(Transform slot, DungeonDataSO dungeonDataSo)
     {
         _slot = slot;
         _config = new MapGenerationConfig(dungeonDataSo);
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
         _tileDataDict = _config.TileMappingDataSO?.tileMappings
             .Where(mapping => mapping.tileData != null)
             .ToDictionary(mapping => mapping.cellType, mapping => mapping.tileData);
     }
 
     public abstract void GenerateMap(int seed);
     
     protected void InitializeGrid()
     {
         _grid = new CellType[_config.GridSize.x, _config.GridSize.y];
         _floorList = new List<RectInt>();
         _connectedRoomPairs.Clear();
         _roomConnections.Clear();
     }
     
     protected void ExpandPath()
     {
         var pathsToExpand = new List<Vector2Int>();
         for (var x = 1; x < _config.GridSize.x - 1; x++)
         {
             for (var y = 1; y < _config.GridSize.y - 1; y++)
             {
                 if (_grid[x, y] == CellType.Path)
                 {
                     pathsToExpand.Add(new Vector2Int(x, y));
                 }
             }
         }
 
         foreach (var pos in pathsToExpand)
         {
             SetNeighborsToExpandedPath(pos.x, pos.y);
         }
     }
 
     private void SetNeighborsToExpandedPath(int x, int y)
     {
         int[] dx = { -1, 1, 0, 0 };
         int[] dy = { 0, 0, -1, 1 };
 
         for (int i = 0; i < 4; i++)
         {
             int nx = x + dx[i];
             int ny = y + dy[i];
             
             if (IsValid(new Vector2Int(nx, ny)) && _grid[nx, ny] == CellType.Empty)
             {
                 _grid[nx, ny] = CellType.ExpandedPath;
             }
         }
     }
     
     protected void BuildWalls() => CheckAndSetNeighborWalls(CellType.Floor, CellType.Empty, CellType.Wall);
     
     protected void BuildPathWalls() => CheckAndSetNeighborWalls(CellType.Empty, CellType.ExpandedPath, CellType.PathWall);
     
     private void CheckAndSetNeighborWalls(CellType centerType, CellType neighborType, CellType wallType)
     {
         for (int x = 1; x < _config.GridSize.x - 1; x++)
         {
             for (int y = 1; y < _config.GridSize.y - 1; y++)
             {
                 if (_grid[x, y] == centerType)
                 {
                     SetNeighborWalls(x, y, neighborType, wallType);
                 }
             }
         }
     }
 
     private void SetNeighborWalls(int x, int y, CellType neighborType, CellType wallType)
     {
         int[] dx = { -1, 1, 0, 0 };
         int[] dy = { 0, 0, -1, 1 };
 
         for (int i = 0; i < 4; i++)
         {
             int nx = x + dx[i];
             int ny = y + dy[i];
 
             if (_grid[nx, ny] == neighborType)
             {
                 _grid[x, y] = wallType;
                 return;
             }
         }
     }
     
     protected void BuildGate()
     {
         Func<Vector2Int, bool> isGateCandidate2 = pos =>
             _grid[pos.x, pos.y] == CellType.Wall &&
             GetNeighborCells(pos)
                 .Any(cell => cell == CellType.Floor || cell == CellType.FloorCenter) &&
             GetNeighborCells(pos)
                 .Any(cell => cell == CellType.ExpandedPath) &&
             GetNeighborCells(pos)
                 .Any(cell => cell == CellType.Gate) && 
             GetNeighborCells(pos)
                 .Any(cell => cell == CellType.Wall || cell == CellType.PathWall) && 
             GetNeighborCells(pos)
                 .Any(cell => cell != CellType.Empty);
 
         ProcessGrid(isGateCandidate2, CellType.Gate);
     }
 
     private void ProcessGrid(Func<Vector2Int, bool> condition, CellType newType)
     {
         var cellsToUpdate = new List<Vector2Int>();
         for (int x = 1; x < _config.GridSize.x - 1; x++)
         {
             for (int y = 1; y < _config.GridSize.y - 1; y++)
             {
                 var pos = new Vector2Int(x, y);
                 if (condition(pos))
                 {
                     cellsToUpdate.Add(pos);
                 }
             }
         }
 
         foreach (var pos in cellsToUpdate)
         {
             _grid[pos.x, pos.y] = newType;
         }
     }
 
     private IEnumerable<CellType> GetNeighborCells(Vector2Int pos)
     {
         return GetNeighbors(pos)
             .Where(IsValid)
             .Select(p => _grid[p.x, p.y]);
     }
 
     private Vector2Int[] GetNeighbors(Vector2Int pos) => new[]
     {
         pos + Vector2Int.left, pos + Vector2Int.right,
         pos + Vector2Int.down, pos + Vector2Int.up
     };
 
     private bool IsValid(Vector2Int pos) =>
         pos.x >= 0 && pos.x < _config.GridSize.x && pos.y >= 0 && pos.y < _config.GridSize.y;
 
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
         if (_tileDataDict == null)
         {
             tileData = null;
             return false;
         }
         return _tileDataDict.TryGetValue(cellType, out tileData) && tileData != null;
     }
 
     public virtual MapData GetMapData()
     {
         return new MapData
         {
             grid = _grid,
             floorList = _floorList,
             gridSize = _config.GridSize,
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
          
          foreach (var edge in edges.Where(e => !selectedEdges.Contains(e)))  
         { 
             if (edge.Distance > 30f)  
             { 
                 selectedEdges.Add(edge); 
             } 
         }  
           
         _connectedRoomPairs.Clear(); 
         _roomConnections.Clear(); 
      
         for (int i = 0; i < _floorList.Count; i++) 
         { 
             _roomConnections[i] = new HashSet<int>(); 
         } 
      
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
                 continue; 
             } 
          
             // 간접 연결 가능한지 체크 (경로가 2개 이하인 경우만) 
             if (CanReachIndirectly(startRoomIndex, endRoomIndex, 2)) 
             { 
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
         DungeonPathfinder2D aStar = new DungeonPathfinder2D(_config.GridSize); 
          
         var path = aStar.FindPath( 
             startPos,  
             endPos,  
             (pathNode, currentNode) =>  
             {  
                 var baseCost = Vector2Int.Distance(currentNode.Position, endPos); 
                 var traversalCost = _grid[currentNode.Position.x, currentNode.Position.y] switch 
                 { 
                     CellType.Path => 0.1f,
                     CellType.Floor => 1f, 
                     CellType.Empty => 5f, 
                     CellType.Wall => 10f, 
                     _ => 4f 
                 }; 
                 
                 float directionCost = CalculateDirectionChangeCost(pathNode, currentNode, endPos);
                 
                 return new DungeonPathfinder2D.PathCost 
                 { 
                     traversable = true, 
                     cost = traversalCost + directionCost
                 }; 
             }); 

         if (path == null) return; 

         foreach (var pos in path)  
         { 
             if (_grid[pos.x, pos.y] == CellType.Empty)  
             { 
                 _grid[pos.x, pos.y] = CellType.Path; 
             }
             else if (_grid[pos.x, pos.y] == CellType.Wall)
             {
                 _grid[pos.x, pos.y] = CellType.Gate;
             }
         } 
     } 

     private void CreateStraightPath(Vector2Int startPos, Vector2Int endPos) 
     { 
         DungeonPathfinder2D aStar = new DungeonPathfinder2D(_config.GridSize); 
          
         var path = aStar.FindPath( 
             startPos,  
             endPos,  
             (pathNode, currentNode) =>  
             {  
                 var baseCost = Mathf.Abs(currentNode.Position.x - endPos.x) + Mathf.Abs(currentNode.Position.y - endPos.y);
                 
                 float directionCost = CalculateDirectionChangeCost(pathNode, currentNode, endPos);
                  
                 return new DungeonPathfinder2D.PathCost 
                 { 
                     traversable = true, 
                     cost = baseCost + directionCost
                 }; 
             }); 

         if (path == null) return; 

         foreach (var pos in path)  
         { 
             if (_grid[pos.x, pos.y] == CellType.Empty)  
             { 
                 _grid[pos.x, pos.y] = CellType.Path; 
             } 
             else if (_grid[pos.x, pos.y] == CellType.Wall)
             {
                 _grid[pos.x, pos.y] = CellType.Gate;
             }
         } 
     } 
     
     private float CalculateDirectionChangeCost(DungeonPathfinder2D.Node pathNode, DungeonPathfinder2D.Node currentNode, Vector2Int endPos)
     {
         if (pathNode == null) return 0f;
    
         // 방향 변경 패널티
         float directionChangePenalty = 1000f;
        
         Vector2Int previousDirection = GetDirection(pathNode.Position, currentNode.Position);
         Vector2Int currentDirection = GetDirection(currentNode.Position, endPos);
                
         if (previousDirection != currentDirection)
         {
             return directionChangePenalty;
         }
        
         return 0f; 
     }

     private Vector2Int GetDirection(Vector2Int from, Vector2Int to)
     {
         Vector2Int diff = to - from;
    
         // x, y 차이 중 더 큰 값을 기준으로 방향 결정
         if (Mathf.Abs(diff.x) > Mathf.Abs(diff.y))
         {
             // 좌우 방향
             return new Vector2Int(diff.x > 0 ? 1 : -1, 0);
         }
         else
         {
             // 상하 방향 (x, y 차이가 같을 경우 상하를 우선함)
             return new Vector2Int(0, diff.y > 0 ? 1 : -1);
         }
     }

      
     #endregion 
      
     
     private int FindRoomIndex(RectInt room)
     {
         // Use a more efficient lookup if needed, but linear search is fine for smaller lists
         return _floorList.IndexOf(room);
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
         // Store connection info
         var roomPair = startRoomIndex < endRoomIndex
             ? (startRoomIndex, endRoomIndex)
             : (endRoomIndex, startRoomIndex);
         _connectedRoomPairs.Add(roomPair);
         
         // Update bidirectional connection maps
         if (!_roomConnections.ContainsKey(startRoomIndex)) _roomConnections[startRoomIndex] = new HashSet<int>();
         if (!_roomConnections.ContainsKey(endRoomIndex)) _roomConnections[endRoomIndex] = new HashSet<int>();
         
         _roomConnections[startRoomIndex].Add(endRoomIndex);
         _roomConnections[endRoomIndex].Add(startRoomIndex);
 
         // Generate the path
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
 }
 
 // Helper class to encapsulate configuration data
 public class MapGenerationConfig
 {
     public Vector2Int GridSize { get; }
     public Vector3 CubeSize { get; }
     public PathType PathType { get; }
     public int RoomSize { get; }
     public int Margin { get; }
     public float BaseCostWeight { get; }
     public float DirectionChangePenalty { get; }
     public float WallPenalty { get; }
     public TileMappingDataSO TileMappingDataSO { get; }
 
     public MapGenerationConfig(DungeonDataSO dungeonDataSo)
     {
         GridSize = dungeonDataSo.gridSize;
         CubeSize = dungeonDataSo.cubeSize;
         PathType = dungeonDataSo.pathType;
         RoomSize = dungeonDataSo.roomSize;
         Margin = 3; // Hardcoded, consider making configurable
         BaseCostWeight = 5f; // Hardcoded
         DirectionChangePenalty = 5f; // Hardcoded
         WallPenalty = 100f; // Hardcoded
         TileMappingDataSO = dungeonDataSo.tileMappingDataSO;
     }
 }