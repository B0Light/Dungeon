using System.Collections.Generic;
using UnityEngine;

public class GridMovementController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public bool allowDiagonalMovement = false;
    public bool smoothMovement = true;
    
    [Header("Debug")]
    public bool showDebugPath = true;
    public Color pathColor = Color.green;
    
    private MapGridPathfinder _pathfinder;
    private List<Vector2Int> _currentPath;
    private int _currentPathIndex;
    private bool _isMoving;
    private Vector3 _targetWorldPosition;
    private Vector2Int _currentGridPosition;
    
    // 그리드 좌표와 월드 좌표 변환을 위한 설정
    public Vector3 cellSize = Vector3.one;
    public Vector3 gridOffset = Vector3.zero;

    public void Initialize(MapGridPathfinder pathfinder, Vector2Int startPosition)
    {
        _pathfinder = pathfinder;
        _pathfinder.AllowDiagonalMovement = allowDiagonalMovement;
        
        _currentGridPosition = startPosition;
        transform.position = GridToWorldPosition(startPosition);
        _targetWorldPosition = transform.position;
    }

    public bool MoveTo(Vector2Int targetPosition)
    {
        if (_pathfinder == null)
        {
            Debug.LogError("Pathfinder not initialized!");
            return false;
        }

        var path = _pathfinder.FindPath(_currentGridPosition, targetPosition);
        
        if (path == null || path.Count == 0)
        {
            Debug.LogWarning($"No path found from {_currentGridPosition} to {targetPosition}");
            return false;
        }

        _currentPath = path;
        _currentPathIndex = 0;
        _isMoving = false;
        
        if (showDebugPath)
        {
            _pathfinder.PrintPath(path);
        }
        
        StartMovement();
        return true;
    }

    private void Update()
    {
        if (_isMoving && smoothMovement)
        {
            HandleSmoothMovement();
        }
    }

    private void StartMovement()
    {
        if (_currentPath == null || _currentPathIndex >= _currentPath.Count)
        {
            return;
        }

        var nextGridPos = _currentPath[_currentPathIndex];
        _targetWorldPosition = GridToWorldPosition(nextGridPos);
        
        if (smoothMovement)
        {
            _isMoving = true;
        }
        else
        {
            // 즉시 이동
            transform.position = _targetWorldPosition;
            _currentGridPosition = nextGridPos;
            OnReachedWaypoint();
        }
    }

    private void HandleSmoothMovement()
    {
        transform.position = Vector3.MoveTowards(
            transform.position, 
            _targetWorldPosition, 
            moveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, _targetWorldPosition) < 0.01f)
        {
            transform.position = _targetWorldPosition;
            _currentGridPosition = _currentPath[_currentPathIndex];
            _isMoving = false;
            
            OnReachedWaypoint();
        }
    }

    private void OnReachedWaypoint()
    {
        _currentPathIndex++;
        
        if (_currentPathIndex >= _currentPath.Count)
        {
            // 목적지 도달
            OnPathCompleted();
        }
        else
        {
            // 다음 웨이포인트로 이동
            StartMovement();
        }
    }

    private void OnPathCompleted()
    {
        Debug.Log($"Reached destination: {_currentGridPosition}");
        _currentPath = null;
        _currentPathIndex = 0;
    }

    private Vector3 GridToWorldPosition(Vector2Int gridPos)
    {
        return new Vector3(
            gridPos.x * cellSize.x + gridOffset.x,
            gridOffset.y,
            gridPos.y * cellSize.z + gridOffset.z
        );
    }

    public Vector2Int WorldToGridPosition(Vector3 worldPos)
    {
        return new Vector2Int(
            Mathf.RoundToInt((worldPos.x - gridOffset.x) / cellSize.x),
            Mathf.RoundToInt((worldPos.z - gridOffset.z) / cellSize.z)
        );
    }

    // 유틸리티 메서드들
    public bool IsPositionWalkable(Vector2Int gridPosition)
    {
        return _pathfinder?.IsPositionWalkable(gridPosition) ?? false;
    }

    public Vector2Int GetCurrentGridPosition()
    {
        return _currentGridPosition;
    }

    public bool IsMoving()
    {
        return _isMoving || _currentPath != null;
    }

    // 디버그 드로잉
    private void OnDrawGizmos()
    {
        if (!showDebugPath || _currentPath == null) return;

        Gizmos.color = pathColor;
        
        for (int i = 0; i < _currentPath.Count - 1; i++)
        {
            var current = GridToWorldPosition(_currentPath[i]);
            var next = GridToWorldPosition(_currentPath[i + 1]);
            
            Gizmos.DrawLine(current, next);
            Gizmos.DrawWireSphere(current, 0.2f);
        }
        
        if (_currentPath.Count > 0)
        {
            var last = GridToWorldPosition(_currentPath[_currentPath.Count - 1]);
            Gizmos.DrawWireSphere(last, 0.2f);
        }
    }
}