using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AISpawnManager : MonoBehaviour
{
    public static AISpawnManager Instance { get; private set; }
    
    private readonly List<AICharacterSpawner_AStar> _aiCharacterSpawners_AStar = new List<AICharacterSpawner_AStar>();
    private readonly List<AICharacterSpawner_Navmesh> _aiCharacterSpawners_Navmesh = new List<AICharacterSpawner_Navmesh>();
    private readonly List<AICharacterManager> _spawnedInCharacters = new List<AICharacterManager>();
    
    private GridPathfinder _pathfinder;
    private Dictionary<int, HashSet<int>> _roomConnections;
    private void Awake()
    {
        Instance = this;
    }

    public void Init(GridPathfinder pathfinder, Dictionary<int, HashSet<int>> roomConnections)
    {
        _pathfinder = pathfinder;
        _roomConnections = roomConnections;
        StartCoroutine(InitialSpawnSequence());
    }
    
    
    // 게임 시작 시 초기 스폰 시퀀스
    private IEnumerator InitialSpawnSequence()
    {
        // 스포너들이 등록될 때까지 잠시 대기
        yield return new WaitForSeconds(0.5f);
        
        if (_aiCharacterSpawners_AStar.Count == 0 &&  _aiCharacterSpawners_Navmesh.Count == 0)
        {
            Debug.LogWarning("No spawners registered! Waiting for spawners...");
            yield return new WaitUntil(() => _aiCharacterSpawners_AStar.Count + _aiCharacterSpawners_Navmesh.Count > 0);
        }
        
        yield return StartCoroutine(SpawnEnemies());
    }
    
    private IEnumerator SpawnEnemies()
    {
        foreach (var spawner in _aiCharacterSpawners_AStar)
        {
            yield return new WaitForEndOfFrame();
            spawner.SpawnUnit(_pathfinder, SetPatrolRoute(GetCurRoomIndex(spawner.Init())));
        }
        foreach (var spawner in _aiCharacterSpawners_Navmesh)
        {
            yield return new WaitForEndOfFrame();
            spawner.SpawnUnit(SetPatrolRoute(GetCurRoomIndex(spawner.Init())));
        }
    }
    
    
    public void RegisterSpawner(AICharacterSpawner_AStar aiCharacterSpawnerAStar)
    {
        _aiCharacterSpawners_AStar.Add(aiCharacterSpawnerAStar);
    }
    
    public void RegisterSpawner(AICharacterSpawner_Navmesh aiCharacterSpawnerNavmesh)
    {
        _aiCharacterSpawners_Navmesh.Add(aiCharacterSpawnerNavmesh);
    }

    public void AddCharacterToSpawnedCharactersList(AICharacterManager character)
    {
        if(_spawnedInCharacters.Contains(character))
            return;
        
        _spawnedInCharacters.Add(character);
    }
    
    private void DespawnAllCharacters()
    {
        foreach (var character in _spawnedInCharacters)
        {
            Destroy(character.gameObject);
        }
        _spawnedInCharacters.Clear();
    }

    #region Generate Route

    private Dictionary<int, HashSet<int>> BuildMST(Dictionary<int, HashSet<int>> graph, int start)
    {
        var mst = graph.Keys.ToDictionary(
            k => k,
            k => new HashSet<int>()
        );

        var visited = new HashSet<int>();
        visited.Add(start);

        var pq = new List<(int from, int to)>();

        foreach (var n in graph[start])
            pq.Add((start, n));

        while (pq.Count > 0)
        {
            var edge = pq[0];
            pq.RemoveAt(0);

            int from = edge.from;
            int to = edge.to;

            if (visited.Contains(to)) continue;

            // Add edge to MST
            mst[from].Add(to);
            mst[to].Add(from);

            visited.Add(to);

            foreach (var next in graph[to])
            {
                if (!visited.Contains(next))
                    pq.Add((to, next));
            }
        }

        return mst;
    }
    
    private List<int> DFSVisit(Dictionary<int, HashSet<int>> graph, int start)
    {
        var visited = new HashSet<int>();
        var order = new List<int>();

        void DFS(int node)
        {
            visited.Add(node);
            order.Add(node);

            foreach (var next in graph[node])
            {
                if (!visited.Contains(next))
                    DFS(next);
            }
        }

        DFS(start);
        return order;
    }

    private List<Vector2Int> SetPatrolRoute(int startRoom)
    {
        // 1. MST 만들기
        var mst = BuildMST(_roomConnections, startRoom);

        // 2. MST를 DFS로 돌며 모든 방 방문
        List<int> visitOrder = DFSVisit(mst, startRoom);
        List<Vector2Int> route = new List<Vector2Int>();
        
        foreach (int roomId in visitOrder)
        {
            var targetRoom = _pathfinder.RoomList[roomId];
            route.Add(new Vector2Int((int)targetRoom.center.x, (int)targetRoom.center.y));
        }

        return route;
    }

    private int GetCurRoomIndex(Vector2Int pos)
    {
        for (int i = 0; i < _pathfinder.RoomList.Count; i++)
        {
            if (_pathfinder.RoomList[i].Contains(pos))
                return i;
        }

        return -1;
    }

    #endregion
    
    

}