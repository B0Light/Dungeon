using UnityEngine;
using UnityEngine.Serialization;

public class AICharacterPursueManager : MonoBehaviour
{
    private AICharacterManager _aiCharacter;
    [HideInInspector] public CharacterManager pursueTarget;
    
    [HideInInspector] public float distanceFromTarget;
    [HideInInspector] public float viewableAngle;
    [HideInInspector] public Vector3 targetDirection;
    
    [Header("Detection")]
    [SerializeField] float detectionRadius = 15;
    public float minimumFOV = -35;
    public float maximumFOV = 35;
    public float attackRange = 2;
    public float rotationSpeed = 25;
    
    private readonly int _maxDetectionCount = 10;
    private Collider[] _colliderBuffer;
    
    private void Awake()
    {
        _aiCharacter = GetComponent<AICharacterManager>();
    }

    public void SetTarget(CharacterManager newTarget)
    {
        pursueTarget = newTarget;
    }
    
     public virtual void FindTargetViaLineOfSight(AICharacterManager curCharacter)
    {
        // 이미 타겟이 있다면 조기 반환
        if (pursueTarget != null)
            return;

        // 캐릭터 위치 캐싱
        Vector3 searchPosition = curCharacter.transform.position;
        
        // 클래스 멤버 변수로 선언하여 GC 압박 감소
        if (_colliderBuffer == null || _colliderBuffer.Length != _maxDetectionCount)
            _colliderBuffer = new Collider[_maxDetectionCount];

        // 레이어 마스크 캐싱
        int characterLayer = WorldUtilityManager.Instance.GetCharacterLayer();
        
        // 감지된 콜라이더 수 확인
        int hitCount = Physics.OverlapSphereNonAlloc(searchPosition, detectionRadius, _colliderBuffer, characterLayer);

        CharacterManager bestTarget = null;
        float closestDistance = float.MaxValue;
        
        // hitCount만큼만 순회하여 성능 최적화
        for (int i = 0; i < hitCount; i++)
        {
            var col = _colliderBuffer[i];
            
            // null 체크 추가
            if (col == null)
                continue;

            // 컴포넌트 캐싱으로 중복 호출 방지
            CharacterManager targetCharacter = col.GetComponent<CharacterManager>();

            // 유효성 검사들을 단계별로 수행 (비용이 적은 것부터)
            if (!IsValidTarget(targetCharacter))
                continue;

            // 거리 기반 우선순위를 위한 거리 계산
            float distanceToTarget = Vector3.Distance(searchPosition, targetCharacter.transform.position);
            
            // 시야각 검사 (레이캐스트보다 비용이 적음)
            if (!IsTargetInFieldOfView(targetCharacter))
                continue;

            // 가장 비용이 큰 라인 오브 사이트 검사를 마지막에
            if (!HasLineOfSight(targetCharacter))
                continue;

            // 가장 가까운 타겟 선택
            if (distanceToTarget < closestDistance)
            {
                bestTarget = targetCharacter;
                closestDistance = distanceToTarget;
            }
        }

        // 배열 초기화로 다음 프레임에서 이전 데이터 참조 방지
        for (int i = 0; i < hitCount; i++)
        {
            _colliderBuffer[i] = null;
        }

        // 최적의 타겟 설정
        if (bestTarget != null)
        {
            SetTargetWithViewableAngle(bestTarget);
        }
        else
        {
            SetTarget(null);
        }
    }
     
    private bool IsValidTarget(CharacterManager targetCharacter)
    {
        return targetCharacter != null && 
               targetCharacter != _aiCharacter && 
               !targetCharacter.isDead.Value;
    }
    
     // 타겟이 시야각 내에 있는지 확인하는 메서드
    private bool IsTargetInFieldOfView(CharacterManager targetCharacter)
    {
        Vector3 targetsDirection = targetCharacter.transform.position - _aiCharacter.transform.position;
        float angleOfPotentialTarget = Vector3.Angle(targetsDirection, _aiCharacter.transform.forward);
        
        return angleOfPotentialTarget > minimumFOV && angleOfPotentialTarget < maximumFOV;
    }

    // 타겟과의 시선이 차단되지 않았는지 확인하는 메서드
    private bool HasLineOfSight(CharacterManager targetCharacter)
    {
        Vector3 aiLockOnPosition = _aiCharacter.lockOnPosition;
        Vector3 targetLockOnPosition = targetCharacter.lockOnPosition;
        
        bool isBlocked = Physics.Linecast(aiLockOnPosition, targetLockOnPosition, WorldUtilityManager.Instance.GetEnvLayer());
        
        if (isBlocked)
        {
            Debug.DrawLine(aiLockOnPosition, targetLockOnPosition);
        }
        
        return !isBlocked;
    }

    // 타겟을 설정하고 각도를 계산하는 메서드
    private void SetTargetWithViewableAngle(CharacterManager targetCharacter)
    {
        Vector3 targetsDirection = targetCharacter.transform.position - transform.position;
        viewableAngle = WorldUtilityManager.Instance.GetAngleOfTarget(transform, targetsDirection);
        SetTarget(targetCharacter);
    }
    
    public void RotateTowardsAgent(AICharacterManager target)
    {
        if (target.aiCharacterVariableManager.CLVM.velocity.magnitude > 0.1f)
        {
            target.transform.rotation = target.navMeshAgent.transform.rotation;
        }
        else
        {
            Vector3 direction = pursueTarget.transform.position - target.transform.position;
            direction.y = 0;
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
                target.transform.rotation = Quaternion.Slerp(target.transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }
}
