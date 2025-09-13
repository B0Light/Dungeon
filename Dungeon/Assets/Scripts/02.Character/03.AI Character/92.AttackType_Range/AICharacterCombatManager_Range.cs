using System;
using UnityEngine;

public class AICharacterCombatManager_Range : AICharacterCombatManager
{
    [Header("Basic Projectiles")]
    [SerializeField] protected ProjectileType projectileType;
    [SerializeField] protected Transform firePoint;
    
    [Header("Grenade Settings")]
    [SerializeField] protected bool canThrowGrenades = false;
    [SerializeField] protected ProjectileType grenadeType = ProjectileType.Grenade;
    [SerializeField] protected Transform grenadeThrowPoint;
    [SerializeField] protected float grenadeThrowForce = 15f;
    [SerializeField] protected float grenadeThrowAngle = 45f;
    [SerializeField] protected float grenadeRange = 10f;
    [SerializeField] protected float grenadeCooldown = 3f;
    
    [Header("Debug Settings")]
    [SerializeField] protected bool enableGrenadeDebug = true;
    [SerializeField] protected bool enableCollisionDebug = true;
    
    [Header("Advanced Targeting")]
    [SerializeField] protected bool useAdvancedTargeting = true;
    [SerializeField] protected float targetPredictionFactor = 0.3f; // 타겟 움직임 예측
    [SerializeField] protected Vector3 targetOffset = Vector3.zero; // 타겟 오프셋 (발 근처로 조정)
    
    // Private fields for grenade system
    private float _lastGrenadeTime = -999f;
    
    private void Start()
    {
        if (firePoint == null) firePoint = transform;
        if (grenadeThrowPoint == null) grenadeThrowPoint = firePoint;
    }

    /* Animation Events */
    public virtual void FireProjectile()
    {
        // projectilePoolManager.FireAtTarget(projectileType, firePoint.position, currentTarget.transform, firePoint);
        UnifiedProjectilePoolManager.Instance.FireInDirection(aiCharacter, projectileType, firePoint.position, firePoint.forward, firePoint);
    }
    
    /// <summary>
    /// 수류탄을 던지는 메서드 - 애니메이션 이벤트로 호출
    /// </summary>
    public virtual void ThrowGrenade()
    {
        if (!canThrowGrenades || currentTarget == null) return;
        
        // 쿨다운 체크
        if (Time.time - _lastGrenadeTime < grenadeCooldown) return;
        
        // 향상된 타겟팅 시스템 사용
        Vector3 targetPosition = useAdvancedTargeting ? 
            CalculateAdvancedTargetPosition(currentTarget) : 
            currentTarget.transform.position;
            
        Vector3 throwDirection = CalculateGrenadeThrowDirection(targetPosition);
        
        // 방향 벡터 검증
        if (throwDirection.magnitude < 0.1f)
        {
            return;
        }
        
        // 수류탄 발사 전 ProjectileConfiguration 검증
        if (!ValidateGrenadeConfiguration())
        {
            return;
        }
        
        // 충돌 디버깅이 활성화되어 있으면 향상된 발사 메서드 사용
        if (enableCollisionDebug)
        {
            FireGrenadeWithCollisionFix(throwDirection);
        }
        else
        {
            UnifiedProjectilePoolManager.Instance.FireInDirection(
                aiCharacter, 
                grenadeType, 
                grenadeThrowPoint.position, 
                throwDirection, 
                grenadeThrowPoint
            );
        }
        
        _lastGrenadeTime = Time.time;
    }
    
    /// <summary>
    /// 수류탄을 특정 위치로 던지는 메서드
    /// </summary>
    public virtual void ThrowGrenadeAtPosition(Vector3 targetPosition)
    {
        if (!canThrowGrenades) return;
        
        // 쿨다운 체크
        if (Time.time - _lastGrenadeTime < grenadeCooldown) return;
        
        Vector3 throwDirection = CalculateGrenadeThrowDirection(targetPosition);
        
        UnifiedProjectilePoolManager.Instance.FireInDirection(
            aiCharacter, 
            grenadeType, 
            grenadeThrowPoint.position, 
            throwDirection, 
            grenadeThrowPoint
        );
        
        _lastGrenadeTime = Time.time;
    }
    
    /// <summary>
    /// 수류탄 던지기가 가능한지 확인
    /// </summary>
    public virtual bool CanThrowGrenade()
    {
        if (!canThrowGrenades) return false;
        if (currentTarget == null) return false;
        if (Time.time - _lastGrenadeTime < grenadeCooldown) return false;
        
        float distanceToTarget = Vector3.Distance(transform.position, currentTarget.transform.position);
        return distanceToTarget <= grenadeRange;
    }
    
    /// <summary>
    /// 수류탄 던지기 방향을 계산 (간단하고 정확한 포물선 궤도)
    /// </summary>
    private Vector3 CalculateGrenadeThrowDirection(Vector3 targetPosition)
    {
        Vector3 toTarget = targetPosition - grenadeThrowPoint.position;
        float horizontalDistance = new Vector3(toTarget.x, 0, toTarget.z).magnitude;
        float heightDifference = toTarget.y;
        
        // 타겟이 너무 가까우면 단순히 위쪽으로 던지기
        if (horizontalDistance < 1f)
        {
            return (Vector3.up * 0.8f + transform.forward * 0.2f).normalized;
        }
        
        Vector3 horizontalDirection = new Vector3(toTarget.x, 0, toTarget.z).normalized;
        
        // 거리에 따른 적응적 각도 계산 (간단한 방법)
        float adaptiveAngle = CalculateAdaptiveAngle(horizontalDistance, heightDifference);
        
        // 방향 벡터 계산 (수평 성분 + 수직 성분)
        Vector3 horizontalComponent = horizontalDirection * Mathf.Cos(adaptiveAngle * Mathf.Deg2Rad);
        Vector3 verticalComponent = Vector3.up * Mathf.Sin(adaptiveAngle * Mathf.Deg2Rad);
        
        Vector3 throwDirection = horizontalComponent + verticalComponent;
        
        return throwDirection.normalized;
    }
    
    /// <summary>
    /// 거리와 높이 차이에 따른 적응적 각도 계산 (수정됨)
    /// </summary>
    private float CalculateAdaptiveAngle(float horizontalDistance, float heightDifference)
    {
        // 기본 각도 - 더 높은 각도로 시작
        float baseAngle = 45f; // 기본값을 45도로 고정
        
        // 거리별 각도 조정 (더 보수적으로)
        if (horizontalDistance < 3f)
        {
            // 가까운 거리: 매우 높은 각도
            baseAngle = 55f;
        }
        else if (horizontalDistance < 6f)
        {
            // 중간 거리: 적당한 각도
            baseAngle = 45f;
        }
        else if (horizontalDistance < 10f)
        {
            // 긴 거리: 약간 낮은 각도
            baseAngle = 35f;
        }
        else
        {
            // 매우 먼 거리: 낮은 각도
            baseAngle = 30f;
        }
        
        // 높이 차이 보정 (더 보수적으로)
        if (heightDifference < -1f) // 타겟이 아래에 있으면
        {
            // 높이가 많이 차이날 때는 각도를 조금만 낮춤
            float heightAdjustment = Mathf.Abs(heightDifference) * 2f; // 1m당 2도만 조정
            baseAngle = Mathf.Max(baseAngle - heightAdjustment, 25f); // 최소 25도 유지
        }
        else if (heightDifference > 1f) // 타겟이 위에 있으면
        {
            // 각도를 높임
            float heightAdjustment = heightDifference * 3f;
            baseAngle += heightAdjustment;
        }
        
        // 각도 제한 (최소 25도, 최대 70도)
        float finalAngle = Mathf.Clamp(baseAngle, 25f, 70f);
        
        return finalAngle;
    }
    
    /// <summary>
    /// 향상된 타겟 위치 계산 (움직임 예측 + 오프셋)
    /// </summary>
    private Vector3 CalculateAdvancedTargetPosition(CharacterManager target)
    {
        Vector3 currentPosition = target.transform.position;
        
        // 타겟의 움직임 예측
        Vector3 velocity = Vector3.zero;
        
        // 타겟이 CharacterVariableManager를 가지고 있으면 속도 정보 가져오기
        if (target.characterVariableManager != null && target.characterVariableManager.CLVM != null)
        {
            // 이동 방향과 속도 계산
            Vector3 moveDirection = target.characterVariableManager.CLVM.moveDirection;
            float moveSpeed = target.characterVariableManager.CLVM.walkSpeed;
            
            // 현재 이동 중이면 속도 벡터 계산
            if (moveDirection.magnitude > 0.1f)
            {
                velocity = moveDirection.normalized * moveSpeed;
            }
        }
        
        // 수류탄이 도달하는 시간 추정 (거리 / 속도)
        float distance = Vector3.Distance(grenadeThrowPoint.position, currentPosition);
        float projectileSpeed = 15f; // 기본값, 실제로는 ProjectileConfiguration에서 가져와야 함
        float timeToTarget = distance / projectileSpeed;
        
        // 예측 위치 계산
        Vector3 predictedPosition = currentPosition + velocity * timeToTarget * targetPredictionFactor;
        
        // 타겟 오프셋 적용 (발 근처로 조정)
        Vector3 finalTargetPosition = predictedPosition + targetOffset;
        
        return finalTargetPosition;
    }
    
    /// <summary>
    /// 수류탄 설정을 검증하는 메서드
    /// </summary>
    private bool ValidateGrenadeConfiguration()
    {
        if (UnifiedProjectilePoolManager.Instance == null)
        {
            return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// 수류탄에 최적화된 FireInDirection 메서드
    /// </summary>
    public void FireGrenadeWithCollisionFix(Vector3 throwDirection)
    {
        // 기본 발사
        UnifiedProjectilePoolManager.Instance.FireInDirection(
            aiCharacter, 
            grenadeType, 
            grenadeThrowPoint.position, 
            throwDirection, 
            grenadeThrowPoint
        );
        
        // 충돌 감시 코루틴 시작 (옵션)
        if (enableCollisionDebug)
        {
            StartCoroutine(MonitorGrenadeCollision(grenadeThrowPoint.position, throwDirection));
        }
    }
    
    /// <summary>
    /// 수류탄 충돌을 모니터링하는 코루틴
    /// </summary>
    private System.Collections.IEnumerator MonitorGrenadeCollision(Vector3 startPos, Vector3 direction)
    {
        float elapsedTime = 0f;
        float maxTime = 5f; // 5초간 모니터링
        
        while (elapsedTime < maxTime)
        {
            // 0.1초마다 충돌 체크
            yield return new WaitForSeconds(0.1f);
            elapsedTime += 0.1f;
            
            // 현재 위치에서 주변 충돌체 검사
            Vector3 currentPos = startPos + direction * (elapsedTime * 15f); // 대략적인 위치 계산
            
            Collider[] nearbyColliders = Physics.OverlapSphere(currentPos, 1f);
            if (nearbyColliders.Length > 0)
            {
                break;
            }
        }
    }
    
    /// <summary>
    /// 디버그용: 수류탄 던지기 범위를 시각화
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (!canThrowGrenades || !enableGrenadeDebug) return;
        
        // 수류탄 범위 표시
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, grenadeRange);
        
        // 던지는 지점 표시
        if (grenadeThrowPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(grenadeThrowPoint.position, 0.3f);
            
            // 던지는 지점에서 앞 방향 표시
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(grenadeThrowPoint.position, grenadeThrowPoint.forward * 2f);
        }
        
        // 현재 타겟이 있으면 던지기 궤도 예상선 표시
        if (currentTarget != null && CanThrowGrenade())
        {
            // 향상된 타겟팅 적용
            Vector3 actualTargetPos = useAdvancedTargeting ? 
                CalculateAdvancedTargetPosition(currentTarget) : 
                currentTarget.transform.position;
            
            Vector3 throwDir = CalculateGrenadeThrowDirection(actualTargetPos);
            
            // 던지기 방향 표시
            Gizmos.color = Color.green;
            Gizmos.DrawRay(grenadeThrowPoint.position, throwDir * 8f);
            
            // 포물선 궤도 시뮬레이션 (간단 버전)
            DrawTrajectoryPreview(grenadeThrowPoint.position, throwDir);
            
            // 실제 타겟 위치 vs 예측 타겟 위치 표시
            if (useAdvancedTargeting)
            {
                // 실제 타겟 (빨간색)
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(currentTarget.transform.position, 0.5f);
                
                // 예측 타겟 (파란색)
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(actualTargetPos, 0.5f);
                
                // 예측 라인
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(currentTarget.transform.position, actualTargetPos);
            }
            
            // 타겟과의 거리 표시
            Gizmos.color = Color.white;
            Gizmos.DrawLine(grenadeThrowPoint.position, actualTargetPos);
        }
    }
    
    /// <summary>
    /// 수류탄 궤도 미리보기를 그리는 메서드
    /// </summary>
    private void DrawTrajectoryPreview(Vector3 startPos, Vector3 initialVelocity)
    {
        Vector3 currentPos = startPos;
        Vector3 velocity = initialVelocity * grenadeThrowForce;
        float timeStep = 0.1f;
        int maxSteps = 50;
        
        Gizmos.color = Color.magenta;
        
        for (int i = 0; i < maxSteps; i++)
        {
            Vector3 nextPos = currentPos + velocity * timeStep;
            velocity += Physics.gravity * timeStep;
            
            Gizmos.DrawLine(currentPos, nextPos);
            currentPos = nextPos;
            
            // 지면에 닿으면 중단
            if (currentPos.y <= startPos.y - 10f) break;
        }
    }
}