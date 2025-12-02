using UnityEngine;

public class AICharacterPursueManager : MonoBehaviour
{
    private AICharacterManager _aiCharacter;
    
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
        Debug.Log($"AI Set Target : {newTarget?.name}");
        _aiCharacter.SetTarget(newTarget);
        _aiCharacter.characterVariableManager.CLVM.isSprinting = newTarget;
        _aiCharacter.navMeshAgent.stoppingDistance = attackRange;
    }
    
     public virtual void FindTargetViaLineOfSight(AICharacterManager curCharacter)
    {
        // 이미 타겟이 있다면 조기 반환
        if (_aiCharacter.CurrentTarget != null) return;

        Vector3 searchPosition = curCharacter.transform.position;
        
        if (_colliderBuffer == null || _colliderBuffer.Length != _maxDetectionCount)
            _colliderBuffer = new Collider[_maxDetectionCount];

        int characterLayer = WorldUtilityManager.Instance.GetCharacterLayer();
        
        int hitCount = Physics.OverlapSphereNonAlloc(searchPosition, detectionRadius, _colliderBuffer, characterLayer);

        CharacterManager bestTarget = null;
        float closestDistance = float.MaxValue;
        
        for (int i = 0; i < hitCount; i++)
        {
            var col = _colliderBuffer[i];
            
            if (col == null)
                continue;

            CharacterManager targetCharacter = col.GetComponent<CharacterManager>();

            if (!IsValidTarget(targetCharacter))
                continue;

            float distanceToTarget = Vector3.Distance(searchPosition, targetCharacter.transform.position);
            
            if (!IsTargetInFieldOfView(targetCharacter))
                continue;

            if (!HasLineOfSight(targetCharacter))
                continue;

            if (distanceToTarget < closestDistance)
            {
                bestTarget = targetCharacter;
                closestDistance = distanceToTarget;
            }
        }

        for (int i = 0; i < hitCount; i++)
        {
            _colliderBuffer[i] = null;
        }

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
        return targetCharacter != null && // 존재해야함
               targetCharacter != _aiCharacter && // 내가 아니여야 함
               !targetCharacter.isDead.Value && // 살아있어야 함
               targetCharacter.characterGroup != _aiCharacter.characterGroup; // 아군이 아니여야 함
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
    
    public void RotateTowardsAgent()
    {
        if (_aiCharacter.aiCharacterVariableManager.CLVM.velocity.magnitude > 0.1f)
        { 
            transform.rotation = _aiCharacter.navMeshAgent.transform.rotation;
        }
        else
        {
            Vector3 direction = _aiCharacter.CurrentTarget.transform.position - transform.position;
            direction.y = 0;
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }
}
