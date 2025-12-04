using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public enum TurretState
{
    Idle,       // 타겟 없음: 주변을 일정 속도로 회전
    Targeting,  // 타겟 있음, 조준 중: 타겟을 향해 부드럽게 회전
    Attacking   // 타겟 조준 완료: 공격 쿨다운을 확인하고 발사
}

public class TurretFSM : MonoBehaviour
{
    [SerializeField] private Transform turretMesh;
    [SerializeField] private WorldUtilityManager.CharacterGroup characterGroup;
    [SerializeField] private SphereCollider sphereCollider;
    
    [Header("Turret Info")]
    [SerializeField] private float rotationSpeed = 50f; // Idle 상태에서 회전 속도
    [SerializeField] private float turnSpeed = 10f;     // Targeting 상태에서 조준 속도 (Slerp factor)
    [SerializeField] private float fireRate = 1f;       // 초당 공격 횟수
    [SerializeField] private float attackRange = 10f;
    
    private TurretState _currentState = TurretState.Idle;
    
    // --- 내부 로직 변수 ---
    private Transform _currentTarget;                           // 현재 공격 대상의 Transform
    private readonly List<Transform> _targetsInRange = new List<Transform>(); // 사거리 내 적 리스트
    private float _fireCooldown = 0f;                         // 다음 공격까지 남은 시간

    private void Start()
    {
        sphereCollider.isTrigger = true;
        sphereCollider.radius = attackRange;
        // 시작 시 초기 상태 설정
        ChangeState(TurretState.Idle);
    }

    private void Update()
    {
        if (_fireCooldown > 0)
        {
            _fireCooldown -= Time.deltaTime;
        }

        switch (_currentState)
        {
            case TurretState.Idle:
                UpdateIdleState();
                break;
            case TurretState.Targeting:
                UpdateTargetingState();
                break;
            case TurretState.Attacking:
                UpdateAttackingState();
                break;
        }
    }

    private void ChangeState(TurretState newState)
    {
        // if (_currentState == newState) return;

        // OnStateExit(_currentState);
        _currentState = newState;
        // OnStateEnter(_currentState);
    }

    #region State Update

    private void UpdateIdleState()
    {
        CheckForTarget();
        if (_currentTarget != null)
        {
            ChangeState(TurretState.Targeting);
            return;
        }

        turretMesh.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }

    private void UpdateTargetingState()
    {
        if (_currentTarget == null)
        {
            ChangeState(TurretState.Idle);
            return;
        }

        Vector3 direction = _currentTarget.position - turretMesh.position;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            turretMesh.rotation = Quaternion.Slerp(turretMesh.rotation, lookRotation, Time.deltaTime * turnSpeed);
        }
        
        ChangeState(TurretState.Attacking); 
    }
    
    private void UpdateAttackingState()
    {
        if (_currentTarget == null)
        {
            ChangeState(TurretState.Idle);
            return;
        }
        
        if (_fireCooldown <= 0f)
        {
            Shoot();
            _fireCooldown = 1f / fireRate;
        }

        ChangeState(TurretState.Targeting); 
    }

    #endregion
    

    private void CheckForTarget()
    {
        if (_currentTarget != null)
        {
            if (!_targetsInRange.Contains(_currentTarget))
            {
                _currentTarget = null;
            }
        }

        if (_currentTarget == null && _targetsInRange.Count > 0)
        {
            _currentTarget = _targetsInRange[0];
        }
    }
    
    private void Shoot()
    {
        Debug.Log($"[{gameObject.name}] Target Found! Attacking {_currentTarget.name}");
    }

    private bool IsOpponent(Transform target)
    {
        CharacterManager targetCharacter = target.GetComponent<CharacterManager>();

        if (targetCharacter != null && targetCharacter.characterGroup != this.characterGroup)
        {
            return true;
        }
        return false;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (IsOpponent(other.transform))
        {
            if (!_targetsInRange.Contains(other.transform))
            {
                _targetsInRange.Add(other.transform);
                
                if (_currentState == TurretState.Idle)
                {
                    CheckForTarget();
                    if (_currentTarget != null)
                    {
                        ChangeState(TurretState.Targeting);
                    }
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        _targetsInRange.Remove(other.transform);

        if (_currentTarget == other.transform)
        {
            _currentTarget = null;
        }
    }
}