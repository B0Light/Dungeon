using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(AStarPathfindingSystem))]
public class ShelterVisitor : MonoBehaviour
{
    private AStarPathfindingSystem _aStarPathfindingSystem;
    [SerializeField] private float rotationSpeed = 5f; // 회전 속도
    [SerializeField] private float moveSpeed = 5f; // 이동 속도

    [SerializeField] private int numOfAttraction = 3;
    private PlacedObject _destination;
    
    private Variable<bool> _isMoving = new Variable<bool>(false); // 이동 중인지 확인
    private Animator _animator;
    private readonly int _movementHash = Animator.StringToHash("isMove");
    private readonly int _rideHash = Animator.StringToHash("isRide");
    private readonly int _speedHash = Animator.StringToHash("Speed");
    private ShelterManager _shelterManager;
    private void Awake()
    {
        _aStarPathfindingSystem = GetComponent<AStarPathfindingSystem>();
        _animator = GetComponentInChildren<Animator>();
    }

    private void OnEnable()
    {
        if(_animator == null) 
            _animator = GetComponentInChildren<Animator>();
        _animator.SetFloat(_speedHash, moveSpeed);
        _isMoving.OnValueChanged += b => _animator?.SetBool(_movementHash, b);
    }

    public void SpawnVisitor(ShelterManager shelterManager)
    {
        _shelterManager = shelterManager;
        Vector2Int startPos = new Vector2Int(3, 0);
        if (!SetRoute(startPos, SelectRandomAttraction()))
        {
            GetNextDestination(startPos);
        }
    }
    
    private Vector2Int SelectRandomAttraction()
    {
        var attractions = GridBuildingSystem.Instance.AttractionEntrancePosList;
        if(attractions.Count == 0) 
        { 
            Debug.Log("NO ATTRACTION : GOAL - H.Q");
            return new Vector2Int(4, 1); // H.Q Position;
        }
        int randomIndex = UnityEngine.Random.Range(0, attractions.Count);
        return attractions[randomIndex];
    }

    private bool SetRoute(Vector2Int startPos, Vector2Int goalPos)
    {
        List<Vector3> routePosList = new List<Vector3>();
        List<GridCell> paths = _aStarPathfindingSystem.NavigatePath(startPos, goalPos);
        if (paths != null)
        {
            foreach (var gridRoute in paths)
            {
                _destination = gridRoute.GetPlacedObject();
                routePosList.Add(_destination.GetEntrancePoint());
            }
            StartMoving(routePosList);
            return true;
        }
        else
        {
            Debug.Log("NO WAY GOAL : " + goalPos);
            // 선택한 목적지로 가는 경로가 없는 경우 
            return false;
        }
    }
    
    private void StartMoving(List<Vector3> points)
    {
       StartCoroutine(MoveThroughPointsCoroutine(points));
    }
    
    private IEnumerator MoveThroughPointsCoroutine(List<Vector3> points)
    {
        foreach (Vector3 targetPoint in points)
        {
            yield return StartCoroutine(MoveToPointCoroutine(targetPoint));
        }
        TryAttraction();
    }
    
    public IEnumerator MoveToPointCoroutine(Vector3 targetPoint, Quaternion? targetRotation = null)
    {
        yield return StartCoroutine(RotateTowardsPointCoroutine(targetPoint));
        _isMoving.Value = true;
        // Move until very close to the target
        while (Vector3.Distance(transform.position, targetPoint) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPoint, moveSpeed * Time.deltaTime);
            yield return null; // Wait until next frame
        }

        // Ensure precise positioning
        transform.position = targetPoint;
        
        if (targetRotation.HasValue)
        {
            transform.rotation = targetRotation.Value;
        }

        _isMoving.Value = false;
    }
    
    private IEnumerator RotateTowardsPointCoroutine(Vector3 targetPoint)
    {
        if (targetPoint == transform.position) yield break;
        
        Vector3 direction = (targetPoint - transform.position).normalized;

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        while (Quaternion.Angle(transform.rotation, targetRotation) > 1f)
        {
            _isMoving.Value = false;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            yield return null; 
        }
        transform.rotation = targetRotation;
    }

    
    private void TryAttraction()
    {
        if (!_destination) return;
        
        RevenueFacilityTile revenueFacilityTile = _destination.gameObject.GetComponent<RevenueFacilityTile>();
        
        if(!revenueFacilityTile) return;
        
        revenueFacilityTile.AddVisitor(this);
    }
    
    public void GetNextDestination(Vector2Int curPos)
    {
        Debug.Log("Set NEXT DESTINATION");
        if(numOfAttraction > 0)
        {
            numOfAttraction--;
            if (!SetRoute(curPos, SelectRandomAttraction()))
            {
                GetNextDestination(curPos);
            }
        }
        else
        {
            if (!SetRoute(curPos, GridBuildingSystem.Instance.GetEntrancePos()))
            {
                Destroy(this.gameObject);
            }
        }
    }

    public void RideAttraction()
    {
        _animator.SetBool(_rideHash, true);
        _animator.CrossFade("Ride", 0.2f);
    }

    public void ExitAttraction()
    {
        _animator.SetBool(_rideHash, false);
    }

    public void LeaveShelter()
    {
        _shelterManager.RemoveVisitor(this);
        Destroy(gameObject);
    }
}
