using UnityEngine;

[CreateAssetMenu(menuName = "A.I/States/PursueTarget")]
public class PursueTargetState : AIState
{
    private float pursuitStartTime; // 추격 시작 시간
    private const float PursuitTimeout = 10f; // 추격 시간 제한 (10초)
    private float targetDistance;
    private const float DistanceLimit = 3;

    public override void OnEnterState(AICharacterManager aiCharacter)
    {
        base.OnEnterState(aiCharacter);
        pursuitStartTime = Time.time; // 상태 진입 시 시간 초기화
    }

    public override AIState Tick(AICharacterManager aiCharacter)
    {
        if (aiCharacter.isPerformingAction) return this;

        // 목표가 없으면 Idle 상태로 전환
        if (aiCharacter.CurrentTarget == null)
            return SwitchState(aiCharacter, aiCharacter.stateIdle);

        // 추격 시간 초과 시 목표 제거 후 Idle 상태로 전환
        if (Time.time - pursuitStartTime > PursuitTimeout)
        {
            aiCharacter.aiCharacterPursueManager.SetTarget(null);
            return SwitchState(aiCharacter, aiCharacter.stateIdle);
        }

        // NavMeshAgent 활성화
        if (!aiCharacter.navMeshAgent.enabled)
            aiCharacter.navMeshAgent.enabled = true;
        
        aiCharacter.aiCharacterLocomotionManager.RotateTowardAgent(aiCharacter);
        
        // 타겟과의 거리 계산
        aiCharacter.aiCharacterPursueManager.targetDirection =
            aiCharacter.CurrentTarget.transform.position - aiCharacter.transform.position;
        aiCharacter.aiCharacterPursueManager.viewableAngle = 
            WorldUtilityManager.Instance.GetAngleOfTarget(aiCharacter.transform, aiCharacter.aiCharacterPursueManager.targetDirection);
        targetDistance = Vector3.Distance(aiCharacter.transform.position, aiCharacter.CurrentTarget.transform.position);
        aiCharacter.aiCharacterPursueManager.distanceFromTarget = targetDistance;
        
        // 타겟과의 거리 확인
        if (aiCharacter.aiCharacterPursueManager.distanceFromTarget <=
            aiCharacter.attackRange)
        {
            Debug.Log("Target is within attack range");
            return SwitchState(aiCharacter, aiCharacter.stateCombatStance);
        }

        // 경로 설정
        //Debug.Log($"Set Destination : {aiCharacter.CurrentTarget.name}");
        aiCharacter.navMeshAgent.SetDestination(aiCharacter.CurrentTarget.transform.position);
        aiCharacter.aiCharacterVariableManager.CLVM.isSprinting = targetDistance >= DistanceLimit;
        aiCharacter.navMeshAgent.stoppingDistance = 1f;
        return this;
    }
}
