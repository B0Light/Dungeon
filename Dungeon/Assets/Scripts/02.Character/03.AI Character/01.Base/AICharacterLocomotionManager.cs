using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class AICharacterLocomotionManager : CharacterLocomotionManager
{
    [Header("AI Components")]
    private AICharacterManager _aiCharacterManager;
    protected NavMeshAgent navAgent;

    [SerializeField] private bool isHumanoid = true;
    
    private readonly int _isMove = Animator.StringToHash("isMove");
    
    [Header("AI Movement Settings")]
    [SerializeField] private float destinationChangeThreshold = 1.0f;
    [SerializeField] private float maxNavMeshSearchDistance = 50f;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = false;
    [SerializeField] private bool debugNavMeshPath = false;
    [SerializeField] private bool debugCLVMValues = false;
    
    // Initialization flags
    private bool isAIInitialized = false;
    
    // Debug tracking
    private Vector3 lastTargetPosition;
    private float lastSpeed2D = -1f;
    private GaitState lastGaitState = GaitState.Idle;

    protected override void UpdateAnimatorController()
    {
        if (isHumanoid)
        {
            base.UpdateAnimatorController();
        }
        else
        {
            characterManager.animator.SetBool(_isMove, CLVM.movementInputHeld);
        }
    }

    #region Unity Lifecycle & Initialization
    
    protected override void Awake()
    {
        base.Awake();
        InitializeAIComponents();
    }
    
    protected override void Start()
    {
        base.Start();
        StartCoroutine(SafeAIInitialization());
    }
    
    private void InitializeAIComponents()
    {
        _aiCharacterManager = characterManager as AICharacterManager;
        
        if (_aiCharacterManager == null)
        {
            Debug.LogError($"[AI Locomotion] AICharacterManager not found on {gameObject.name}!");
            return;
        }
        
        navAgent = _aiCharacterManager.navMeshAgent;
        
        if (navAgent == null)
        {
            Debug.LogError($"[AI Locomotion] NavMeshAgent not found on {gameObject.name}!");
        }
    }
    
    private IEnumerator SafeAIInitialization()
    {
        // CLVM과 다른 컴포넌트들이 초기화될 때까지 대기
        yield return new WaitUntil(() => IsSafeToOperate());
        
        // 한 프레임 더 대기
        yield return null;
        
        isAIInitialized = true;
        
        if (showDebugInfo)
            Debug.Log($"[AI Locomotion] {gameObject.name} initialized successfully");
    }
    
    private bool IsSafeToOperate()
    {
        return _aiCharacterManager != null && 
               navAgent != null && 
               navAgent.isActiveAndEnabled &&
               characterManager != null && 
               characterManager.characterVariableManager != null && 
               CLVM != null;
    }
    
    private bool IsNavMeshAgentSafe()
    {
        return navAgent != null && 
               navAgent.isActiveAndEnabled && 
               navAgent.isOnNavMesh;
    }
    
    #endregion

    #region State Management
    
    protected override void EnterState(AnimationState stateToEnter)
    {
        if (!IsSafeToOperate())
        {
            if (showDebugInfo)
                Debug.LogWarning($"[AI Locomotion] Cannot enter state {stateToEnter} - not safe to operate");
            return;
        }
        
        base.EnterState(stateToEnter);
        
        CLVM.currentState = stateToEnter;
        
        switch (CLVM.currentState)
        {
            case AnimationState.Base:
                EnterBaseState();
                break;
            case AnimationState.Locomotion:
                EnterAILocomotionState();
                break;
            case AnimationState.Jump:
                EnterJumpState();
                break;
            case AnimationState.Fall:
                EnterFallState();
                break;
            case AnimationState.Crouch:
                EnterCrouchState();
                break;
            case AnimationState.DoubleJump:
                EnterJumpState();
                break;
            case AnimationState.Dead:
                EnterAIDeadState();
                break;
        }
        
        if (showDebugInfo)
            Debug.Log($"[AI Locomotion] Entered state: {stateToEnter}");
    }

    protected override void ExitCurrentState()
    {
        if (!IsSafeToOperate()) return;
        
        switch (CLVM.currentState)
        {
            case AnimationState.Locomotion:
                ExitLocomotionState();
                break;
            case AnimationState.Jump:
                ExitJumpState();
                break;
            case AnimationState.Crouch:
                ExitCrouchState();
                break;
        }
    }
    
    protected override void EnterLocomotionState()
    {
        // AI 전용 로코모션 진입 로직
        if (showDebugInfo)
            Debug.Log("[AI Locomotion] Entered Locomotion State");
    }
    
    private void EnterAILocomotionState()
    {
        EnterLocomotionState();
    }
    
    protected override void ExitLocomotionState()
    {
        // AI 전용 로코모션 종료 로직
        if (showDebugInfo)
            Debug.Log("[AI Locomotion] Exited Locomotion State");
    }
    
    protected override void EnterCrouchState()
    {
        if (showDebugInfo)
            Debug.Log("[AI Locomotion] Entered Crouch State");
    }
    
    protected override void ExitCrouchState()
    {
        if (showDebugInfo)
            Debug.Log("[AI Locomotion] Exited Crouch State");
    }
    
    private void EnterAIDeadState()
    {
        canMove = false;
        canRotate = false;
        
        if (IsNavMeshAgentSafe())
        {
            navAgent.ResetPath();
            navAgent.enabled = false;
        }
        
        if (showDebugInfo)
            Debug.Log("[AI Locomotion] Entered Dead State");
    }
    
    #endregion
    
    #region Rotation & Direction
    
    public void RotateTowardAgent(AICharacterManager aiCharacter)
    {
        if (!IsSafeToOperate() || aiCharacter == null) return;
        
        if (aiCharacter.aiCharacterVariableManager.CLVM.velocity.magnitude > 0.1f)
        {
            aiCharacter.transform.rotation = aiCharacter.navMeshAgent.transform.rotation;
        }
    }

    protected override void CalculateMoveDirection()
    {
        if (!IsSafeToOperate() || !isAIInitialized || !IsNavMeshAgentSafe()) return;
        
        // NavMeshAgent의 desired velocity를 이동 방향으로 설정
        Vector3 desiredVelocity = navAgent.desiredVelocity.normalized;
        CLVM.moveDirection = desiredVelocity;
        CLVM.movementInputHeld = desiredVelocity.magnitude > 0.1f;
        
        // 베이스 클래스의 속도 계산 로직 호출
        base.CalculateMoveDirection();
        
        // 디버그 정보
        if (debugCLVMValues && HasValueChanged())
        {
            DebugCLVMValues();
        }
    }

    protected override void FaceMoveDirection()
    {
        if (!IsSafeToOperate()) return;
        
        Vector3 characterForward = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
        Vector3 characterRight = new Vector3(transform.right.x, 0f, transform.right.z).normalized;
        Vector3 directionForward = new Vector3(CLVM.moveDirection.x, 0f, CLVM.moveDirection.z).normalized;

        if (_aiCharacterManager.characterCombatManager.currentTarget != null)
        {
            HandleCombatRotation(characterForward, characterRight, directionForward);
        }
        else
        {
            HandlePatrolRotation();
        }
    }
    
    private void HandleCombatRotation(Vector3 characterForward, Vector3 characterRight, Vector3 directionForward)
    {
        Vector3 targetDirection = (_aiCharacterManager.characterCombatManager.currentTarget.transform.position - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

        CLVM.strafeAngle = characterForward != directionForward ? Vector3.SignedAngle(characterForward, directionForward, Vector3.up) : 0f;
        CLVM.isTurningInPlace = false;

        if (CLVM.isStrafing)
        {
            HandleStrafeMovement(characterForward, characterRight, directionForward, targetDirection, targetRotation);
        }
        else
        {
            HandleDirectMovement(targetRotation);
        }
    }
    
    private void HandleStrafeMovement(Vector3 characterForward, Vector3 characterRight, Vector3 directionForward, Vector3 targetDirection, Quaternion targetRotation)
    {
        if (CLVM.moveDirection.magnitude > 0.01)
        {
            if (targetDirection != Vector3.zero)
            {
                CLVM.shuffleDirectionZ = Vector3.Dot(characterForward, directionForward);
                CLVM.shuffleDirectionX = Vector3.Dot(characterRight, directionForward);

                UpdateStrafeDirection(
                    Vector3.Dot(characterForward, directionForward),
                    Vector3.Dot(characterRight, directionForward)
                );
                
                _cameraRotationOffset = Mathf.Lerp(_cameraRotationOffset, 0f, CLVM.rotationSmoothing * Time.deltaTime);

                float targetValue = CLVM.strafeAngle > CLVM.forwardStrafeMinThreshold && CLVM.strafeAngle < CLVM.forwardStrafeMaxThreshold ? 1f : 0f;

                if (Mathf.Abs(CLVM.forwardStrafe - targetValue) <= 0.001f)
                {
                    CLVM.forwardStrafe = targetValue;
                }
                else
                {
                    float t = Mathf.Clamp01(_STRAFE_DIRECTION_DAMP_TIME * Time.deltaTime);
                    CLVM.forwardStrafe = Mathf.SmoothStep(CLVM.forwardStrafe, targetValue, t);
                }
            }

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, CLVM.rotationSmoothing * Time.deltaTime);
        }
        else
        {
            UpdateStrafeDirection(1f, 0f);

            float t = 20 * Time.deltaTime;
            float newOffset = 0f;

            if (characterForward != targetDirection)
            {
                newOffset = Vector3.SignedAngle(characterForward, targetDirection, Vector3.up);
            }

            _cameraRotationOffset = Mathf.Lerp(_cameraRotationOffset, newOffset, t);

            if (Mathf.Abs(_cameraRotationOffset) > 10)
            {
                CLVM.isTurningInPlace = true;
            }
        }
    }
    
    private void HandleDirectMovement(Quaternion targetRotation)
    {
        UpdateStrafeDirection(1f, 0f);
        _cameraRotationOffset = Mathf.Lerp(_cameraRotationOffset, 0f, CLVM.rotationSmoothing * Time.deltaTime);

        CLVM.shuffleDirectionZ = 1;
        CLVM.shuffleDirectionX = 0;

        Vector3 faceDirection = new Vector3(CLVM.velocity.x, 0f, CLVM.velocity.z);

        if (faceDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                CLVM.rotationSmoothing * Time.deltaTime
            );
        }
    }
    
    private void HandlePatrolRotation()
    {
        // 순찰 중일 때는 단순히 이동 방향으로 회전
        if (CLVM.moveDirection.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(CLVM.moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, CLVM.rotationSmoothing * Time.deltaTime);
        }
    }

    #endregion

    #region Capsule Size Management

    protected override void CapsuleCrouchingSize(bool crouching)
    {
        if (!IsSafeToOperate()) return;
        
        BoxCollider characterCollider = _aiCharacterManager.characterCollider as BoxCollider;
        
        if (crouching)
        {
            navAgent.baseOffset = CLVM.capsuleCrouchingCentre;
            navAgent.height = CLVM.capsuleCrouchingHeight;
            
            if (characterCollider != null)
            {
                characterCollider.center = new Vector3(0, CLVM.capsuleCrouchingCentre, 0);
                characterCollider.size = new Vector3(1, CLVM.capsuleCrouchingHeight, 1);
            }
        }
        else
        {
            navAgent.baseOffset = CLVM.capsuleStandingCentre;
            navAgent.height = CLVM.capsuleStandingHeight;
            
            if (characterCollider != null)
            {
                characterCollider.center = new Vector3(0, CLVM.capsuleStandingCentre, 0);
                characterCollider.size = new Vector3(1, CLVM.capsuleStandingHeight, 1);
            }
        }
        
        if (showDebugInfo)
            Debug.Log($"[AI Locomotion] Capsule size changed - Crouching: {crouching}");
    }
    
    #endregion
    
    #region Movement & Navigation
    
    protected override void Move()
    {
        if (!IsSafeToOperate() || !isAIInitialized) return;
        
        // 죽었거나 움직일 수 없는 상태
        if (_aiCharacterManager.isDead.Value || !canMove)
        {
            StopNavigation();
            return;
        }

        // NavMeshAgent 안전성 확인
        if (!IsNavMeshAgentSafe())
        {
            if (showDebugInfo)
                Debug.LogWarning("[AI Locomotion] NavMeshAgent is not safe to operate");
            return;
        }

        // NavMeshAgent 기본 설정
        navAgent.speed = CLVM.currentMaxSpeed;
        navAgent.isStopped = CLVM.isTurningInPlace;

        // 목적지 결정 로직
        Vector3 newTargetPosition = DetermineTargetPosition();
        
        // 목적지가 실제로 변경되었는지 확인 (성능 최적화)
        if (ShouldUpdateDestination(newTargetPosition))
        {
            SetDestinationSafely(newTargetPosition);
        }
        
        if (debugNavMeshPath)
        {
            DebugNavMeshInfo(newTargetPosition);
        }
    }
    
    private Vector3 DetermineTargetPosition()
    {
        Transform currentDestination = _aiCharacterManager.aiCharacterCombatManager.currentTarget?.transform;
        
        if (currentDestination != null)
        {
            navAgent.stoppingDistance = _aiCharacterManager.aiCharacterCombatManager.attackRange;
            return currentDestination.position;
        }
        else
        {
            Vector3? waypoint = _aiCharacterManager.aiCharacterPatrolManager.GetNextWaypoint();
            navAgent.stoppingDistance = 0.5f; // 순찰시 기본 정지 거리
            return waypoint ?? transform.position;
        }
    }
    
    private bool ShouldUpdateDestination(Vector3 newTargetPosition)
    {
        if (!IsNavMeshAgentSafe()) return false;
        
        return !navAgent.hasPath || 
               Vector3.Distance(navAgent.destination, newTargetPosition) > destinationChangeThreshold ||
               Vector3.Distance(lastTargetPosition, newTargetPosition) > destinationChangeThreshold;
    }
    
    private void StopNavigation()
    {
        if (!IsNavMeshAgentSafe()) return;
        
        if (!navAgent.isStopped)
        {
            navAgent.isStopped = true;
            navAgent.ResetPath(); // 경로 초기화로 성능 향상
            
            if (showDebugInfo)
                Debug.Log("[AI Locomotion] Navigation stopped");
        }
    }

    private void SetDestinationSafely(Vector3 targetPosition)
    {
        if (!IsNavMeshAgentSafe())
        {
            if (showDebugInfo)
                Debug.LogWarning("[AI Locomotion] Cannot set destination - NavMeshAgent not safe");
            return;
        }
        
        NavMeshHit hit;

        if (NavMesh.SamplePosition(targetPosition, out hit, maxNavMeshSearchDistance, NavMesh.AllAreas))
        {
            navAgent.SetDestination(hit.position);
            lastTargetPosition = targetPosition;
            
            if (showDebugInfo)
                Debug.Log($"[AI Locomotion] Destination set to: {hit.position}");
        }
        else
        {
            // 유효한 점을 찾지 못한 경우, 현재 위치에서 가장 가까운 NavMesh 지점으로
            if (NavMesh.SamplePosition(transform.position, out hit, maxNavMeshSearchDistance, NavMesh.AllAreas))
            {
                navAgent.SetDestination(hit.position);
                
                if (showDebugInfo)
                    Debug.LogWarning($"[AI Locomotion] Target position invalid, using nearest valid point: {hit.position}");
            }
            else
            {
                // 완전히 실패한 경우
                navAgent.SetDestination(transform.position);
                Debug.LogWarning($"[AI Locomotion] NavMesh 위에 유효한 경로를 찾지 못했습니다. 목적지: {targetPosition}, 오브젝트: {gameObject.name}");
            }
        }
    }

    // 외부에서 명시적으로 목적지를 설정하는 메서드
    public void ForceSetDestination(Vector3 targetPosition)
    {
        if (!IsSafeToOperate() || !IsNavMeshAgentSafe()) 
        {
            Debug.LogWarning("[AI Locomotion] Cannot force set destination - not safe to operate");
            return;
        }
        
        SetDestinationSafely(targetPosition);
    }

    protected override void GroundedCheck()
    {
        if (!IsSafeToOperate()) return;
        
        CLVM.isGrounded = true;
        base.GroundedCheck();
    }
    
    #endregion
    
    #region Public Properties & Methods
    
    public AnimationState CurrentState => IsSafeToOperate() ? CLVM.currentState : AnimationState.Base;
    public GaitState CurrentGait => IsSafeToOperate() ? CLVM.currentGait : GaitState.Idle;
    public float CurrentSpeed => IsSafeToOperate() ? CLVM.speed2D : 0f;
    public bool IsMoving => IsSafeToOperate() && IsNavMeshAgentSafe() && navAgent.hasPath && navAgent.remainingDistance > navAgent.stoppingDistance;
    public bool IsStopped => !IsMoving;
    public bool IsInitialized => isAIInitialized;
    public float RemainingDistance => IsNavMeshAgentSafe() ? navAgent.remainingDistance : 0f;
    
    // 외부에서 이동 제어
    public void SetCanMove(bool canMove)
    {
        this.canMove = canMove;
        
        if (!canMove)
        {
            StopNavigation();
        }
        
        if (showDebugInfo)
            Debug.Log($"[AI Locomotion] CanMove set to: {canMove}");
    }
    
    #endregion
    
    #region Debug & Utilities
    
    private bool HasValueChanged()
    {
        return Mathf.Abs(CLVM.speed2D - lastSpeed2D) > 0.01f || 
               CLVM.currentGait != lastGaitState;
    }
    
    private void DebugCLVMValues()
    {
        Debug.Log($"[CLVM DEBUG] Speed2D: {lastSpeed2D:F2} -> {CLVM.speed2D:F2}, " +
                 $"Gait: {lastGaitState} -> {CLVM.currentGait}, " +
                 $"MoveDirection: {CLVM.moveDirection}");
        
        lastSpeed2D = CLVM.speed2D;
        lastGaitState = CLVM.currentGait;
    }
    
    private void DebugNavMeshInfo(Vector3 targetPosition)
    {
        if (!IsNavMeshAgentSafe())
        {
            Debug.LogWarning("[NavMesh DEBUG] NavMeshAgent is not safe to operate");
            return;
        }
        
        Debug.Log($"[NavMesh DEBUG] HasPath: {navAgent.hasPath}, " +
                 $"RemainingDistance: {navAgent.remainingDistance:F2}, " +
                 $"Target: {targetPosition}, " +
                 $"Velocity: {navAgent.velocity.magnitude:F2}, " +
                 $"IsOnNavMesh: {navAgent.isOnNavMesh}");
    }
    
    #endregion
    
    #region Gizmos
    
    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying || !IsNavMeshAgentSafe()) return;
        
        // 목적지 표시
        if (navAgent.hasPath)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(navAgent.destination, 0.5f);
            
            // 현재 위치에서 목적지까지 선 그리기
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, navAgent.destination);
        }
        
        // NavMeshAgent 경로 표시
        if (navAgent.hasPath)
        {
            Gizmos.color = Color.blue;
            Vector3[] pathCorners = navAgent.path.corners;
            
            for (int i = 0; i < pathCorners.Length - 1; i++)
            {
                Gizmos.DrawLine(pathCorners[i], pathCorners[i + 1]);
            }
        }
        
        // 정지 거리 표시
        if (navAgent.hasPath)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(navAgent.destination, navAgent.stoppingDistance);
        }
        
        // NavMeshAgent 상태 표시
        if (!navAgent.isOnNavMesh)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position, Vector3.one * 2f);
        }
    }
    
    #endregion
}