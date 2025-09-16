using System;
using UnityEngine;
using System.Collections.Generic;

public class MapDataManager : MonoBehaviour
{
    public static MapDataManager Instance;
    
    private Dictionary<int, int> _killLog = new Dictionary<int, int>();
    
    [Header("Waypoint System")]
    [SerializeField]
    private MapGeneratorFactory mapGeneratorFactory;
    
    private WaypointSystemData _waypointSystemData;

    private void Awake()
    {
        Instance = this;
        
        if (mapGeneratorFactory == null)
            mapGeneratorFactory = FindFirstObjectByType<MapGeneratorFactory>();
    }

    private void Start()
    {
        _waypointSystemData = null;
    }

    private void SetWaypointSystemData()
    {
        if (mapGeneratorFactory == null || !mapGeneratorFactory.IsMapGenerated())
        {
            Debug.LogWarning("MapDataManager: 맵이 생성되지 않았거나 MapGeneratorFactory를 찾을 수 없습니다.");
            return;
        }

        _waypointSystemData = mapGeneratorFactory.GetCurrentWaypointSystemData();
        
        if (_waypointSystemData != null)
        {
            Debug.Log($"MapDataManager: 웨이포인트 시스템 데이터 업데이트 완료 " +
                      $"- {_waypointSystemData.waypoints.Count}개 웨이포인트, {_waypointSystemData.patrolRoutes.Count}개 패트롤 경로");
        }
        else
        {
            Debug.LogError("MapDataManager: 웨이포인트 시스템 데이터를 가져올 수 없습니다.");
        }
    }
    
    public WaypointSystemData GetWaypointSystemData()
    {
        if (_waypointSystemData == null)
        {
            SetWaypointSystemData();
        }
        return _waypointSystemData;
    }
    
    public PatrolRoute GetPatrolRoute(string routeName)
    {
        var waypointSystem = GetWaypointSystemData();
        if (waypointSystem?.patrolRoutes == null) return null;
        
        foreach (var route in waypointSystem.patrolRoutes)
        {
            if (route.routeName == routeName)
                return route;
        }
        
        return null;
    }
    
    public List<PatrolRoute> GetAllPatrolRoutes()
    {
        var waypointSystem = GetWaypointSystemData();
        return waypointSystem?.patrolRoutes ?? new List<PatrolRoute>();
    }
    
    public int FindNearestWaypoint(Vector3 position)
    {
        var waypointSystem = GetWaypointSystemData();
        return waypointSystem?.FindNearestWaypoint(position) ?? -1;
    }
    
    public void AddKillLog(int itemId)
    {
        if (_killLog.TryGetValue(itemId, out int currentCount))
        {
            _killLog[itemId] = currentCount + 1;
        }
        else
        {
            _killLog[itemId] = 1;
        }
    }

    public Dictionary<int, int> GetKillLog() => _killLog;
}
