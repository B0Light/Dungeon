using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class AICharacterPatrolManager : MonoBehaviour
{
    private readonly List<Vector2Int> _patrolPointList = new List<Vector2Int>();
    private Vector2Int _curPosition;
    
    [HideInInspector] public Vector3 cellSize = Vector3.one;
    [HideInInspector] public Vector3 gridOffset = Vector3.zero;
    
    public void SetPatrolPoint(List<Vector2Int> patrolPointList, Vector2Int startPos)
    {
        _curPosition = startPos;
        transform.position = new Vector3(GetGridPosition(startPos).x,gridOffset.y,GetGridPosition(startPos).y);
        
        foreach (var gridPos in patrolPointList)
        {
            _patrolPointList.Add(GetGridPosition(gridPos));
        }
    }
    
    private Vector2Int GetGridPosition(Vector2Int gridPos)
    {
        return new Vector2Int(
            Mathf.FloorToInt(gridPos.x * cellSize.x + gridOffset.x),
            Mathf.FloorToInt(gridPos.y * cellSize.z + gridOffset.z)
        );
    }
    
    private Vector3 GetToWorldPosition(Vector2Int gridPos)
    {
        return new Vector3(
            gridPos.x * cellSize.x + gridOffset.x,
            gridOffset.y,
            gridPos.y * cellSize.z + gridOffset.z
        );
    }
    
    
    public Vector3 GetNextWaypoint()
    {
        if (_patrolPointList == null || _patrolPointList.Count == 0)
        {
            Debug.LogError("Patrol point list is empty or null.");
            return GetToWorldPosition(_curPosition); 
        }

        int currentIndex = _patrolPointList.IndexOf(_curPosition);

        if (currentIndex == -1)
        {
            Debug.LogWarning("Current grid position not found in the patrol list. Returning the first point.");
            return GetToWorldPosition(_patrolPointList[0]);
        }

        int nextIndex = (currentIndex + 1) % _patrolPointList.Count;

        return GetToWorldPosition(_patrolPointList[nextIndex]);
    }

    public bool IsInAmbushMode()
    {
        return true;
    }
}