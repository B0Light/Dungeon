using System.Collections;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// AI 캐릭터의 패트롤 행동을 관리하는 매니저 (AIPatrolManager 기능 통합)
/// </summary>
public class AICharacterPatrolManager : MonoBehaviour
{
    private AICharacterManager aiCharacter;

    // 신규 웨이포인트 시스템 사용
    private WaypointSystemData _waypointSystemData;
    private List<Vector3> _currentPatrolRoute;
    private int _currentWaypointIndex = 0;
    private string _assignedRouteName;

    private bool _isMapDataLoaded;
    
    [Header("Ambush Settings")]
    [SerializeField]
    [Range(0f, 1f)]
    private float ambushProbability = 0.3f; // 30% 확률로 매복 모드
    
    [SerializeField]
    private float ambushDuration = 10f; // 매복 지속 시간 (초)
    
    private bool isInAmbushMode = false;
    private Vector3 ambushPosition;
    private float ambushStartTime;
    
    [Header("Patrol Management")]
    [SerializeField] private bool autoAssignPatrolRoutes = true;
    [SerializeField] private bool showWaypointsInScene = true;
    [SerializeField] private float waypointGizmoSize = 0.5f;
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;
    
    // 패트롤 경로 할당 상태 (AIPatrolManager에서 통합)
    private Dictionary<string, List<AICharacterPatrolManager>> routeAssignments = new Dictionary<string, List<AICharacterPatrolManager>>();
    
    private void Start()
    {
        aiCharacter = GetComponent<AICharacterManager>();
        StartCoroutine(WaitForMapDataManager());
        
        // 자동 경로 할당이 활성화되어 있으면 즉시 할당
        if (autoAssignPatrolRoutes)
        {
            StartCoroutine(WaitAndAssignRoute());
        }
    }
    
    private IEnumerator WaitAndAssignRoute()
    {
        yield return new WaitForSeconds(0.5f); // 맵 데이터 로딩 대기
        SelectWaypointData();
    }
    
    private IEnumerator WaitForMapDataManager()
    {
        float waitTime = 0f;
        const float maxWaitTime = 5f;
    
        while (MapDataManager.Instance == null && waitTime < maxWaitTime)
        {
            waitTime += Time.deltaTime;
            yield return null;
        }
    
        if (MapDataManager.Instance != null)
        {
            _waypointSystemData = MapDataManager.Instance.GetWaypointSystemData();
            AssignRandomPatrolRoute();
        }
        else
        {
            Debug.LogWarning("MapDataManager.Instance를 5초 동안 기다렸지만 null입니다.");
        }

        _isMapDataLoaded = true;
    }

    /// <summary>
    /// 랜덤 패트롤 경로 할당
    /// </summary>
    private void AssignRandomPatrolRoute()
    {
        if (_waypointSystemData?.patrolRoutes == null || _waypointSystemData.patrolRoutes.Count == 0)
        {
            Debug.LogWarning($"{name}: 사용 가능한 패트롤 경로가 없습니다.");
            return;
        }

        // 랜덤하게 패트롤 경로 선택
        int randomRouteIndex = Random.Range(0, _waypointSystemData.patrolRoutes.Count);
        PatrolRoute selectedRoute = _waypointSystemData.patrolRoutes[randomRouteIndex];
        
        AssignPatrolRoute(selectedRoute);
    }
    
    /// <summary>
    /// 특정 패트롤 경로 할당
    /// </summary>
    public void AssignPatrolRoute(PatrolRoute route)
    {
        if (route == null || route.waypointIndices.Count == 0)
        {
            Debug.LogWarning($"{name}: 유효하지 않은 패트롤 경로입니다.");
            return;
        }

        // 기존 경로에서 제거
        RemoveFromAllRoutes();

        _assignedRouteName = route.routeName;
        _currentPatrolRoute = new List<Vector3>();
        
        // 웨이포인트 인덱스를 실제 위치로 변환
        foreach (int waypointIndex in route.waypointIndices)
        {
            if (waypointIndex < _waypointSystemData.waypoints.Count)
            {
                _currentPatrolRoute.Add(_waypointSystemData.waypoints[waypointIndex].position);
            }
        }
        
        _currentWaypointIndex = 0;
        
        // 새 경로에 추가
        if (!routeAssignments.ContainsKey(route.routeName))
            routeAssignments[route.routeName] = new List<AICharacterPatrolManager>();
            
        routeAssignments[route.routeName].Add(this);
        
        if (enableDebugLogs)
            Debug.Log($"{name}: 패트롤 경로 '{route.routeName}' 할당됨 ({_currentPatrolRoute.Count}개 웨이포인트)");
        
        // 새로운 웨이포인트 데이터를 선택할 때 매복 모드 여부 결정
        CheckForAmbushMode();
    }
    
    public void SelectWaypointData()
    {
        // 새로운 시스템에서는 패트롤 경로 재할당
        AssignRandomPatrolRoute();
    }
    
    private void CheckForAmbushMode()
    {
        float randomValue = Random.Range(0f, 1f);
        
        if (randomValue <= ambushProbability)
        {
            EnterAmbushMode();
        }
        else
        {
            ExitAmbushMode();
        }
    }
    
    private void EnterAmbushMode()
    {
        isInAmbushMode = true;
        ambushPosition = transform.position;
        ambushStartTime = Time.time;
        
        //Debug.Log($"AI가 매복 모드에 진입했습니다. 위치: {ambushPosition}");
    }
    
    private void ExitAmbushMode()
    {
        /*
        if (isInAmbushMode)
        {
            //Debug.Log("AI가 매복 모드를 종료하고 순찰을 재개합니다.");
        }
        */
        isInAmbushMode = false;
    }
    
    private void Update()
    {
        // 매복 모드 시간 체크
        if (isInAmbushMode && Time.time - ambushStartTime >= ambushDuration)
        {
            ExitAmbushMode();
            SelectWaypointData(); // 매복 종료 후 새로운 웨이포인트 선택
        }
    }

    private bool IsWaypointCompleted()
    {
        if (_currentPatrolRoute == null || _currentPatrolRoute.Count == 0)
        {
            return false;
        }

        Vector3 targetPosition = isInAmbushMode ? ambushPosition : _currentPatrolRoute[_currentWaypointIndex];
        
        float distance = Vector3.Distance(transform.position, targetPosition);
        return distance < 5.0f; 
    }

    public Vector3? GetNextWaypoint()
    {
        // 데이터가 아직 로딩되지 않았으면 null 반환
        if (!_isMapDataLoaded)
        {
            Debug.LogWarning("MapData가 아직 로딩되지 않았습니다.");
            return null;
        }
        
        // 매복 모드일 때는 매복 위치 반환
        if (isInAmbushMode)
        {
            return ambushPosition;
        }
    
        if (_currentPatrolRoute == null || _currentPatrolRoute.Count == 0)
        {
            Debug.LogWarning("NO Patrol Data");
            return null;
        }

        if (IsWaypointCompleted())
        {
            _currentWaypointIndex++;

            if (_currentWaypointIndex >= _currentPatrolRoute.Count)
            {
                SelectWaypointData(); // 새로운 패트롤 경로 선택
            }
        }
        
        // 인덱스 유효성 검사
        if (_currentWaypointIndex >= 0 && _currentWaypointIndex < _currentPatrolRoute.Count)
        {
            return _currentPatrolRoute[_currentWaypointIndex];
        }
        
        return null;
    }
    
    // 현재 AI가 매복 모드인지 확인하는 public 메서드
    public bool IsInAmbushMode()
    {
        return isInAmbushMode;
    }

    /// <summary>
    /// 에이전트를 모든 경로에서 제거
    /// </summary>
    private void RemoveFromAllRoutes()
    {
        foreach (var routeAssignment in routeAssignments)
        {
            routeAssignment.Value.Remove(this);
        }
    }
    
    /// <summary>
    /// 디버그 정보 출력
    /// </summary>
    [ContextMenu("Print Patrol System Info")]
    public void PrintPatrolSystemInfo()
    {
        if (_waypointSystemData == null)
        {
            Debug.Log("웨이포인트 시스템 데이터가 없습니다.");
            return;
        }

        Debug.Log($"=== {name} 패트롤 시스템 정보 ===");
        Debug.Log($"웨이포인트 수: {_waypointSystemData.waypoints.Count}");
        Debug.Log($"패트롤 경로 수: {_waypointSystemData.patrolRoutes.Count}");
        Debug.Log($"할당된 경로: {_assignedRouteName ?? "없음"}");
        Debug.Log($"현재 웨이포인트 인덱스: {_currentWaypointIndex}");
        Debug.Log($"매복 모드: {isInAmbushMode}");
    }
    
    // 에디터에서 웨이포인트 시각화
    private void OnDrawGizmos()
    {
        if (!showWaypointsInScene || _waypointSystemData == null) return;

        // 웨이포인트 그리기
        for (int i = 0; i < _waypointSystemData.waypoints.Count; i++)
        {
            Waypoint waypoint = _waypointSystemData.waypoints[i];
            
            // 웨이포인트 타입별 색상 설정
            switch (waypoint.type)
            {
                case WaypointType.Room:
                    Gizmos.color = Color.green;
                    break;
                case WaypointType.Corridor:
                    Gizmos.color = Color.blue;
                    break;
                case WaypointType.Intersection:
                    Gizmos.color = Color.red;
                    break;
                default:
                    Gizmos.color = Color.yellow;
                    break;
            }

            Gizmos.DrawSphere(waypoint.position, waypointGizmoSize);

            // 연결선 그리기
            Gizmos.color = Color.white;
            foreach (int connectedIndex in waypoint.connectedWaypoints)
            {
                if (connectedIndex < _waypointSystemData.waypoints.Count)
                {
                    Vector3 connectedPosition = _waypointSystemData.waypoints[connectedIndex].position;
                    Gizmos.DrawLine(waypoint.position, connectedPosition);
                }
            }
        }
        
        // 현재 할당된 경로 강조 표시
        if (_assignedRouteName != null && _currentPatrolRoute != null)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < _currentPatrolRoute.Count - 1; i++)
            {
                Gizmos.DrawLine(_currentPatrolRoute[i], _currentPatrolRoute[i + 1]);
            }
            if (_currentPatrolRoute.Count > 0)
            {
                Gizmos.DrawWireSphere(_currentPatrolRoute[_currentWaypointIndex], 1f);
            }
        }
    }
}