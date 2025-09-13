using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "A.I/States/PursueTarget")]
public class PursueTargetState : AIState
{
    private float pursuitStartTime; // 추격 시작 시간
    private const float pursuitTimeout = 10f; // 추격 시간 제한 (10초)

    public override void OnEnterState(AICharacterManager aiCharacter)
    {
        base.OnEnterState(aiCharacter);
        pursuitStartTime = Time.time; // 상태 진입 시 시간 초기화
    }

    public override AIState Tick(AICharacterManager aiCharacter)
    {
        if (aiCharacter.isPerformingAction) return this;

        // 목표가 없으면 Idle 상태로 전환
        if (aiCharacter.aiCharacterCombatManager.currentTarget == null)
            return SwitchState(aiCharacter, aiCharacter.stateIdle);

        // 추격 시간 초과 시 목표 제거 후 Idle 상태로 전환
        if (Time.time - pursuitStartTime > pursuitTimeout)
        {
            aiCharacter.aiCharacterCombatManager.SetTarget(null);
            return SwitchState(aiCharacter, aiCharacter.stateIdle);
        }

        // NavMeshAgent 활성화
        if (!aiCharacter.navMeshAgent.enabled)
            aiCharacter.navMeshAgent.enabled = true;
        
        aiCharacter.aiCharacterLocomotionManager.RotateTowardAgent(aiCharacter);
        
        // 타겟과의 거리 확인
        if (aiCharacter.aiCharacterCombatManager.distanceFromTarget <=
            aiCharacter.navMeshAgent.stoppingDistance)
        {
            return SwitchState(aiCharacter, aiCharacter.stateCombatStance);
        }

        // 경로 설정
        NavMeshPath path = new NavMeshPath();
        aiCharacter.navMeshAgent.CalculatePath(aiCharacter.aiCharacterCombatManager.currentTarget.transform.position, path);
        aiCharacter.navMeshAgent.SetPath(path);

        return this;
    }
}
