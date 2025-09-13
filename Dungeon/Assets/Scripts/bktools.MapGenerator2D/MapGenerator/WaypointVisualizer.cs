using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// 웨이포인트를 Scene에서 시각화하는 도구
/// </summary>
public class WaypointVisualizer : MonoBehaviour
{
    [Header("시각화 설정")]
    [SerializeField] private bool showWaypoints = true;
    [SerializeField] private bool showConnections = true;
    [SerializeField] private bool showPatrolRoutes = false;
    [SerializeField] private bool showWaypointLabels = false;
    
    [Header("크기 및 색상")]
    [SerializeField] private float waypointSize = 0.5f;
    [SerializeField] private float connectionLineWidth = 0.1f;
    [SerializeField] private Color roomWaypointColor = Color.green;
    [SerializeField] private Color corridorWaypointColor = Color.blue;
    [SerializeField] private Color intersectionWaypointColor = Color.red;
    [SerializeField] private Color connectionColor = Color.white;
    [SerializeField] private Color patrolRouteColor = Color.yellow;

    [Header("참조")]
    [SerializeField] private MapGeneratorFactory mapGeneratorFactory;
    
    private WaypointSystemData waypointSystemData;

    private void Start()
    {
        if (mapGeneratorFactory == null)
            mapGeneratorFactory = FindFirstObjectByType<MapGeneratorFactory>();
            
        RefreshWaypointData();
    }

    /// <summary>
    /// 웨이포인트 데이터 새로고침
    /// </summary>
    public void RefreshWaypointData()
    {
        if (mapGeneratorFactory != null && mapGeneratorFactory.IsMapGenerated())
        {
            waypointSystemData = mapGeneratorFactory.GetCurrentWaypointSystemData();
        }
    }

    private void OnDrawGizmos()
    {
        if (!showWaypoints && !showConnections && !showPatrolRoutes) return;
        
        if (waypointSystemData == null)
        {
            RefreshWaypointData();
            if (waypointSystemData == null) return;
        }

        DrawWaypoints();
        
        if (showConnections)
            DrawConnections();
            
        if (showPatrolRoutes)
            DrawPatrolRoutes();
    }

    /// <summary>
    /// 웨이포인트 그리기
    /// </summary>
    private void DrawWaypoints()
    {
        if (!showWaypoints || waypointSystemData.waypoints == null) return;

        for (int i = 0; i < waypointSystemData.waypoints.Count; i++)
        {
            Waypoint waypoint = waypointSystemData.waypoints[i];
            
            // 웨이포인트 타입별 색상 설정
            switch (waypoint.type)
            {
                case WaypointType.Room:
                    Gizmos.color = roomWaypointColor;
                    break;
                case WaypointType.Corridor:
                    Gizmos.color = corridorWaypointColor;
                    break;
                case WaypointType.Intersection:
                    Gizmos.color = intersectionWaypointColor;
                    break;
                default:
                    Gizmos.color = Color.gray;
                    break;
            }

            // 웨이포인트 구체 그리기
            Gizmos.DrawSphere(waypoint.position, waypointSize);
            
            // 웨이포인트 외곽선 그리기
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(waypoint.position, waypointSize);

#if UNITY_EDITOR
            // 레이블 표시
            if (showWaypointLabels)
            {
                Vector3 labelPosition = waypoint.position + Vector3.up * (waypointSize + 0.5f);
                string label = $"{i}\n{waypoint.type}";
                if (waypoint.roomIndex >= 0)
                    label += $"\nRoom:{waypoint.roomIndex}";
                    
                Handles.Label(labelPosition, label);
            }
#endif
        }
    }

    /// <summary>
    /// 웨이포인트 연결선 그리기
    /// </summary>
    private void DrawConnections()
    {
        if (waypointSystemData.waypoints == null) return;

        Gizmos.color = connectionColor;
        
        for (int i = 0; i < waypointSystemData.waypoints.Count; i++)
        {
            Waypoint waypoint = waypointSystemData.waypoints[i];
            
            foreach (int connectedIndex in waypoint.connectedWaypoints)
            {
                if (connectedIndex < waypointSystemData.waypoints.Count && connectedIndex > i)
                {
                    Vector3 start = waypoint.position;
                    Vector3 end = waypointSystemData.waypoints[connectedIndex].position;
                    
                    // 연결선 그리기
                    Gizmos.DrawLine(start, end);
                    
                    // 방향 표시를 위한 작은 화살표
                    Vector3 direction = (end - start).normalized;
                    Vector3 midPoint = Vector3.Lerp(start, end, 0.5f);
                    Vector3 arrowHead1 = midPoint - direction * 0.3f + Vector3.Cross(direction, Vector3.up) * 0.1f;
                    Vector3 arrowHead2 = midPoint - direction * 0.3f - Vector3.Cross(direction, Vector3.up) * 0.1f;
                    
                    Gizmos.DrawLine(midPoint, arrowHead1);
                    Gizmos.DrawLine(midPoint, arrowHead2);
                }
            }
        }
    }

    /// <summary>
    /// 패트롤 경로 그리기
    /// </summary>
    private void DrawPatrolRoutes()
    {
        if (waypointSystemData.patrolRoutes == null) return;

        for (int routeIndex = 0; routeIndex < waypointSystemData.patrolRoutes.Count; routeIndex++)
        {
            PatrolRoute route = waypointSystemData.patrolRoutes[routeIndex];
            
            // 경로별로 색상 변화
            float hue = (float)routeIndex / waypointSystemData.patrolRoutes.Count;
            Gizmos.color = Color.HSVToRGB(hue, 0.8f, 1f);
            
            // 경로의 웨이포인트들을 연결하는 선 그리기
            for (int i = 0; i < route.waypointIndices.Count - 1; i++)
            {
                int currentIndex = route.waypointIndices[i];
                int nextIndex = route.waypointIndices[i + 1];
                
                if (currentIndex < waypointSystemData.waypoints.Count && 
                    nextIndex < waypointSystemData.waypoints.Count)
                {
                    Vector3 start = waypointSystemData.waypoints[currentIndex].position;
                    Vector3 end = waypointSystemData.waypoints[nextIndex].position;
                    
                    // 두꺼운 선으로 패트롤 경로 표시
                    DrawThickLine(start, end, connectionLineWidth);
                }
            }
            
            // 순환 경로인 경우 마지막에서 첫 번째로 연결
            if (route.isLoop && route.waypointIndices.Count > 2)
            {
                int lastIndex = route.waypointIndices[route.waypointIndices.Count - 1];
                int firstIndex = route.waypointIndices[0];
                
                if (lastIndex < waypointSystemData.waypoints.Count && 
                    firstIndex < waypointSystemData.waypoints.Count)
                {
                    Vector3 start = waypointSystemData.waypoints[lastIndex].position;
                    Vector3 end = waypointSystemData.waypoints[firstIndex].position;
                    
                    DrawThickLine(start, end, connectionLineWidth);
                }
            }

#if UNITY_EDITOR
            // 경로 이름 표시
            if (route.waypointIndices.Count > 0 && route.waypointIndices[0] < waypointSystemData.waypoints.Count)
            {
                Vector3 labelPosition = waypointSystemData.waypoints[route.waypointIndices[0]].position + Vector3.up * 1.5f;
                Handles.Label(labelPosition, route.routeName);
            }
#endif
        }
    }

    /// <summary>
    /// 두꺼운 선 그리기
    /// </summary>
    private void DrawThickLine(Vector3 start, Vector3 end, float thickness)
    {
        Vector3 direction = (end - start).normalized;
        Vector3 perpendicular = Vector3.Cross(direction, Vector3.up) * thickness * 0.5f;
        
        // 여러 개의 평행선으로 두꺼운 선 효과
        for (int i = -2; i <= 2; i++)
        {
            Vector3 offset = perpendicular * i * 0.2f;
            Gizmos.DrawLine(start + offset, end + offset);
        }
    }

    /// <summary>
    /// 웨이포인트 정보를 Inspector에서 확인
    /// </summary>
    [ContextMenu("Print Waypoint Info")]
    public void PrintWaypointInfo()
    {
        if (waypointSystemData == null)
        {
            RefreshWaypointData();
            if (waypointSystemData == null)
            {
                Debug.Log("웨이포인트 데이터가 없습니다.");
                return;
            }
        }

        Debug.Log($"=== 웨이포인트 시스템 정보 ===");
        Debug.Log($"총 웨이포인트 수: {waypointSystemData.waypoints.Count}");
        Debug.Log($"총 패트롤 경로 수: {waypointSystemData.patrolRoutes.Count}");

        // 웨이포인트 타입별 개수
        int roomWaypoints = 0, corridorWaypoints = 0, intersectionWaypoints = 0;
        foreach (var waypoint in waypointSystemData.waypoints)
        {
            switch (waypoint.type)
            {
                case WaypointType.Room: roomWaypoints++; break;
                case WaypointType.Corridor: corridorWaypoints++; break;
                case WaypointType.Intersection: intersectionWaypoints++; break;
            }
        }

        Debug.Log($"방 웨이포인트: {roomWaypoints}개");
        Debug.Log($"복도 웨이포인트: {corridorWaypoints}개");
        Debug.Log($"교차로 웨이포인트: {intersectionWaypoints}개");

        // 패트롤 경로 정보
        foreach (var route in waypointSystemData.patrolRoutes)
        {
            Debug.Log($"경로 '{route.routeName}': {route.waypointIndices.Count}개 웨이포인트, 순환: {route.isLoop}");
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// 에디터 전용 - 웨이포인트 시각화 설정 변경 감지
    /// </summary>
    private void OnValidate()
    {
        // 값이 변경되면 Scene View 새로고침
        if (Application.isPlaying)
        {
            RefreshWaypointData();
        }
    }
#endif
}
