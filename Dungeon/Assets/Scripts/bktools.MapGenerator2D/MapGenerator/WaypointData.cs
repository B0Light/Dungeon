using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 웨이포인트 타입 정의
/// </summary>
public enum WaypointType
{
    Room,           // 방 중심점
    Corridor,       // 복도 연결점
    Intersection,   // 교차로
    Door,          // 문 위치
    Custom         // 사용자 정의
}

/// <summary>
/// 단일 웨이포인트 정보
/// </summary>
[System.Serializable]
public class Waypoint
{
    public Vector3 position;        // 월드 좌표
    public Vector2Int gridPosition; // 그리드 좌표
    public WaypointType type;       // 웨이포인트 타입
    public List<int> connectedWaypoints; // 연결된 웨이포인트 인덱스들
    public int roomIndex = -1;      // 소속 방 인덱스 (-1이면 복도)
    public float patrolRadius = 2f; // 패트롤 반경

    public Waypoint(Vector3 worldPos, Vector2Int gridPos, WaypointType waypointType)
    {
        position = worldPos;
        gridPosition = gridPos;
        type = waypointType;
        connectedWaypoints = new List<int>();
    }
}

/// <summary>
/// 패트롤 경로 정보
/// </summary>
[System.Serializable]
public class PatrolRoute
{
    public List<int> waypointIndices; // 패트롤할 웨이포인트 인덱스들
    public bool isLoop;               // 순환 경로 여부
    public float waitTime = 1f;       // 각 웨이포인트에서 대기 시간
    public string routeName;          // 경로 이름

    public PatrolRoute(string name, bool loop = true)
    {
        routeName = name;
        isLoop = loop;
        waypointIndices = new List<int>();
    }
}

/// <summary>
/// 전체 웨이포인트 시스템 데이터
/// </summary>
[System.Serializable]
public class WaypointSystemData
{
    public List<Waypoint> waypoints;         // 모든 웨이포인트들
    public List<PatrolRoute> patrolRoutes;   // 패트롤 경로들
    public Vector2Int gridSize;              // 맵 그리드 크기
    public Vector3 cubeSize;                 // 타일 크기
    
    public WaypointSystemData()
    {
        waypoints = new List<Waypoint>();
        patrolRoutes = new List<PatrolRoute>();
    }

    /// <summary>
    /// 웨이포인트 추가
    /// </summary>
    public int AddWaypoint(Waypoint waypoint)
    {
        waypoints.Add(waypoint);
        return waypoints.Count - 1;
    }

    /// <summary>
    /// 두 웨이포인트 연결
    /// </summary>
    public void ConnectWaypoints(int index1, int index2)
    {
        if (index1 >= 0 && index1 < waypoints.Count && index2 >= 0 && index2 < waypoints.Count)
        {
            if (!waypoints[index1].connectedWaypoints.Contains(index2))
                waypoints[index1].connectedWaypoints.Add(index2);
            
            if (!waypoints[index2].connectedWaypoints.Contains(index1))
                waypoints[index2].connectedWaypoints.Add(index1);
        }
    }

    /// <summary>
    /// 패트롤 경로 추가
    /// </summary>
    public void AddPatrolRoute(PatrolRoute route)
    {
        patrolRoutes.Add(route);
    }

    /// <summary>
    /// 특정 방의 웨이포인트들 가져오기
    /// </summary>
    public List<Waypoint> GetWaypointsInRoom(int roomIndex)
    {
        List<Waypoint> roomWaypoints = new List<Waypoint>();
        foreach (var waypoint in waypoints)
        {
            if (waypoint.roomIndex == roomIndex)
                roomWaypoints.Add(waypoint);
        }
        return roomWaypoints;
    }

    /// <summary>
    /// 가장 가까운 웨이포인트 찾기
    /// </summary>
    public int FindNearestWaypoint(Vector3 position)
    {
        if (waypoints.Count == 0) return -1;

        int nearestIndex = 0;
        float nearestDistance = Vector3.Distance(position, waypoints[0].position);

        for (int i = 1; i < waypoints.Count; i++)
        {
            float distance = Vector3.Distance(position, waypoints[i].position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestIndex = i;
            }
        }

        return nearestIndex;
    }
}
