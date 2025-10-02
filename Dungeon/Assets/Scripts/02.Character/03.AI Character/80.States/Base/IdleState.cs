using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "A.I/States/Base/Idle")]
public class IdleState : AIState
{
    [Header("Patrol Settings")]
    [SerializeField] private float idleWaitTime = 2f; // 웨이포인트 도달 후 대기 시간
    [SerializeField] private float patrolCheckInterval = 0.5f; // 순찰 체크 간격
    
    [Header("Movement Settings")]
    [SerializeField] private float moveStartDelay = 0.2f; // 이동 시작 지연
    [SerializeField] private float destinationReachedThreshold = 2f; // 목적지 도달 판정 거리
    
    [Header("Debug")]
    [SerializeField] private bool debugIdleState = false;
    
    // 상태 추적 변수들
    private float lastPatrolCheckTime = 0f;
    private float idleStartTime = 0f;
    private bool isWaitingAtWaypoint = false;
    private bool isMovingToWaypoint = false;
    private Vector3 lastWaypointPosition;
    private AICharacterLocomotionManager locomotionManager;
    
    public override void OnEnterState(AICharacterManager aiCharacter)
    {
        base.OnEnterState(aiCharacter);
        
        // 컴포넌트 초기화
        InitializeComponents(aiCharacter);
        
        // Idle 상태 진입 시 초기화
        ResetIdleState();
        
        // 현재 이동 중이라면 정지
        StopCurrentMovement(aiCharacter);
        
        if (debugIdleState)
            Debug.Log($"[IdleState] AI {aiCharacter.name} entered Idle state");
    }
    
    public override AIState Tick(AICharacterManager aiCharacter)
    {
        // 죽은 상태면 그대로 유지
        if (aiCharacter.isDead.Value) return this;
        
        // 1순위: 적 탐지 및 추적
        AIState combatState = CheckForCombatTransition(aiCharacter);
        if (combatState != null) return combatState;
        
        // 2순위: 순찰 로직
        HandlePatrolBehavior(aiCharacter);
        
        return this;
    }
    
    protected override void ResetStateFlags(AICharacterManager aiCharacter)
    {
        base.ResetStateFlags(aiCharacter);
        
        // 상태 초기화
        ResetIdleState();
        
        if (debugIdleState)
            Debug.Log($"[IdleState] AI {aiCharacter.name} exited Idle state");
    }
    
    #region Initialization
    
    private void InitializeComponents(AICharacterManager aiCharacter)
    {
        // LocomotionManager 가져오기
        locomotionManager = aiCharacter.GetComponent<AICharacterLocomotionManager>();
        
        if (locomotionManager == null)
        {
            Debug.LogError($"[IdleState] AICharacterLocomotionManager not found on {aiCharacter.name}");
        }
    }
    
    private void ResetIdleState()
    {
        idleStartTime = Time.time;
        isWaitingAtWaypoint = false;
        isMovingToWaypoint = false;
        lastPatrolCheckTime = 0f;
        lastWaypointPosition = Vector3.zero;
    }
    
    #endregion
    
    #region Combat Detection
    
    private AIState CheckForCombatTransition(AICharacterManager aiCharacter)
    {
        // 기존 타겟이 있다면 즉시 추적 상태로 전환
        if (aiCharacter.characterCombatManager.currentTarget != null)
        {
            if (debugIdleState)
                Debug.Log($"[IdleState] Target found, switching to pursue state");
            return SwitchState(aiCharacter, aiCharacter.statePursueTarget);
        }
        
        // 시야 범위 내 적 탐지 시도
        aiCharacter.aiCharacterCombatManager.FindTargetViaLineOfSight(aiCharacter);
        
        // 적이 발견되었다면 추적 상태로 전환
        if (aiCharacter.characterCombatManager.currentTarget != null)
        {
            if (debugIdleState)
                Debug.Log($"[IdleState] Line of sight target found, switching to pursue state");
            return SwitchState(aiCharacter, aiCharacter.statePursueTarget);
        }
        
        return null;
    }
    
    #endregion
    
    #region Patrol Behavior
    
    private void HandlePatrolBehavior(AICharacterManager aiCharacter)
    {
        // LocomotionManager가 없으면 순찰하지 않음
        if (locomotionManager == null || !locomotionManager.IsInitialized) return;
        
        // PatrolManager가 없으면 순찰하지 않음
        AICharacterPatrolManager patrolManager = aiCharacter.GetComponent<AICharacterPatrolManager>();
        if (patrolManager == null) return;
        
        // 매복 모드일 때는 제자리에서 대기
        if (patrolManager.IsInAmbushMode())
        {
            HandleAmbushBehavior(aiCharacter);
            return;
        }
        
        // 일정 간격으로만 순찰 체크 (성능 최적화)
        if (Time.time - lastPatrolCheckTime < patrolCheckInterval)
            return;
            
        lastPatrolCheckTime = Time.time;
        
        // 웨이포인트 도달 후 대기 중인지 확인
        if (isWaitingAtWaypoint)
        {
            if (Time.time - idleStartTime >= idleWaitTime)
            {
                isWaitingAtWaypoint = false;
                StartPatrolling(patrolManager);
            }
            return;
        }
        
        // 현재 목적지로 이동 중인지 확인
        if (isMovingToWaypoint)
        {
            CheckWaypointReached(aiCharacter);
            return;
        }
        
        // 현재 이동 중이 아니라면 순찰 시작
        if (!IsCurrentlyMoving())
        {
            StartPatrolling(patrolManager);
        }
    }
    
    private void HandleAmbushBehavior(AICharacterManager aiCharacter)
    {
        // 매복 모드에서는 제자리에서 대기하며 경계만 함
        if (IsCurrentlyMoving())
        {
            StopCurrentMovement(aiCharacter);
            if (debugIdleState)
                Debug.Log($"[IdleState] AI entering ambush mode, stopping movement");
        }
        
        // 매복 중에도 계속 적을 탐지 (이미 CheckForCombatTransition에서 처리됨)
    }
    
    private void StartPatrolling(AICharacterPatrolManager patrolManager)
    {
        Vector3 waypointPosition = patrolManager.GetNextWaypoint();
        
        if (locomotionManager != null)
        {
            locomotionManager.ForceSetDestination(waypointPosition);
            isMovingToWaypoint = true;
            lastWaypointPosition = waypointPosition;
            
            if (debugIdleState)
                Debug.Log($"[IdleState] AI moving to waypoint: {waypointPosition}");
        }
        
    }
    
    private void CheckWaypointReached(AICharacterManager aiCharacter)
    {
        // 목적지에 도달했는지 확인
        float distanceToWaypoint = Vector3.Distance(aiCharacter.transform.position, lastWaypointPosition);
        
        if (distanceToWaypoint <= destinationReachedThreshold || !IsCurrentlyMoving())
        {
            // 웨이포인트 도달
            OnWaypointReached();
        }
        else if (locomotionManager != null && locomotionManager.RemainingDistance <= aiCharacter.navMeshAgent.stoppingDistance + 0.1f)
        {
            // NavMeshAgent 기준으로도 도달 확인
            OnWaypointReached();
        }
    }
    
    private void OnWaypointReached()
    {
        // 웨이포인트 도달 시 대기 모드 활성화
        isMovingToWaypoint = false;
        isWaitingAtWaypoint = true;
        idleStartTime = Time.time;
        
        if (debugIdleState)
            Debug.Log($"[IdleState] Waypoint reached: {lastWaypointPosition}, starting idle wait");
    }
    
    #endregion
    
    #region Movement Control
    
    private void StopCurrentMovement(AICharacterManager aiCharacter)
    {
        if (locomotionManager != null)
        {
            locomotionManager.SetCanMove(false);
            
            // 잠시 후 다시 이동 가능하도록 설정
            aiCharacter.StartCoroutine(EnableMovementAfterDelay());
        }
    }
    
    private IEnumerator EnableMovementAfterDelay()
    {
        yield return new WaitForSeconds(moveStartDelay);
        
        if (locomotionManager != null)
        {
            locomotionManager.SetCanMove(true);
        }
    }
    
    private bool IsCurrentlyMoving()
    {
        if (locomotionManager == null) return false;
        
        return locomotionManager.IsMoving;
    }
    
    #endregion
}