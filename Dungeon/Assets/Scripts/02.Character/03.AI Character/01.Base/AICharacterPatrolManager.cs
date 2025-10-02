using System;
using UnityEngine;
using System.Collections.Generic;

public class AICharacterPatrolManager : MonoBehaviour
{
    private readonly List<Vector3> _patrolPointList = new List<Vector3>();
    private int _curPatrolPointIndex = 0;
    [HideInInspector] public Vector3 cellSize = Vector3.one;
    [HideInInspector] public Vector3 gridOffset = Vector3.zero;
    
    [SerializeField, Range(0f, 1f)] private float ambushProbability = 0.3f;
    [SerializeField] private float ambushDuration = 10f;
    private bool _isInAmbushMode = false;
    private Vector3 _ambushPosition;
    private float _ambushStartTime;
    
    public void SetPatrolPoint(List<Vector2Int> patrolPointList, Vector2Int startPos)
    {
        foreach (var gridPos in patrolPointList)
        {
            _patrolPointList.Add(GetWorldPositionByGrid(gridPos));
        }
        
        _curPatrolPointIndex = GetClosestPatrolPointIndex(transform.position);
    }
    
    private Vector3 GetWorldPositionByGrid(Vector2Int gridPos)
    {
        return new Vector3(
            gridPos.x * cellSize.x + gridOffset.x,
            gridOffset.y,
            gridPos.y * cellSize.z + gridOffset.z
        );
    }
    
    private int GetClosestPatrolPointIndex(Vector3 currentPosition)
    {
        if (_patrolPointList == null || _patrolPointList.Count == 0)
        {
            Debug.LogWarning("순찰 지점 목록이 비어 있습니다!");
            return -1;
        }
        int closestIndex = -1;
        float minDistanceSqr = Mathf.Infinity; 

        for (int i = 0; i < _patrolPointList.Count; i++)
        {
            float distanceSqr = (_patrolPointList[i] - currentPosition).sqrMagnitude;

            if (distanceSqr < minDistanceSqr)
            {
                minDistanceSqr = distanceSqr;
                closestIndex = i;
            }
        }
        return closestIndex;
    }
    
    public Vector3 GetNextWaypoint()
    {
        if (_isInAmbushMode)
        {
            return _ambushPosition;
        }
        
        if (_patrolPointList == null || _patrolPointList.Count == 0)
        {
            return transform.position;
        }
        
        if (IsWaypointCompleted())
        {
            _curPatrolPointIndex = (_curPatrolPointIndex + 1) % _patrolPointList.Count;
        }

        return _patrolPointList[_curPatrolPointIndex];
    }
    
    private void SelectWaypointData()
    {
        _curPatrolPointIndex = (_curPatrolPointIndex + 1) % _patrolPointList.Count;
        CheckForAmbushMode();
    }
    
    private void CheckForAmbushMode()
    {
        float randomValue = UnityEngine.Random.Range(0f, 1f);
        
        if (randomValue <= ambushProbability)
        {
            EnterAmbushMode();
        }
        else
        {
            ExitAmbushMode();
        }
    }
    
    private bool IsWaypointCompleted()
    {
        if (_patrolPointList.Count == 0)
        {
            return false;
        }
        
        Vector3 targetPosition = _isInAmbushMode ? _ambushPosition : _patrolPointList[_curPatrolPointIndex];
        
        float distance = Vector3.Distance(transform.position, targetPosition);
        return distance < 5.0f; 
    }
    
    private void EnterAmbushMode()
    {
        _isInAmbushMode = true;
        _ambushPosition = transform.position;
        _ambushStartTime = Time.time;
        
        Debug.Log($"AI가 매복 모드에 진입했습니다. 위치: {_ambushPosition}");
    }
    
    private void ExitAmbushMode()
    {
        if (_isInAmbushMode)
        {
            Debug.Log("AI가 매복 모드를 종료하고 순찰을 재개합니다.");
        }
        
        _isInAmbushMode = false;
    }
    
    private void Update()
    {
        // 매복 모드 시간 체크
        if (_isInAmbushMode && Time.time - _ambushStartTime >= ambushDuration)
        {
            ExitAmbushMode();
            SelectWaypointData(); // 매복 종료 후 새로운 웨이포인트 선택
        }
    }

    public bool IsInAmbushMode() => _isInAmbushMode;
}