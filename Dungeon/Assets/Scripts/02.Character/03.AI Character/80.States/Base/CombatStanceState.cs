using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "A.I/States/Combat Stance")]
public class CombatStanceState : AIState
{
    [Header("공격 관련 설정")]
    public List<AICharacterAttackAction> aiCharacterAttacks; // 이 캐릭터가 사용할 수 있는 모든 공격 리스트
    private List<AICharacterAttackAction> _potentialAttacks; // 현재 상황(거리, 각도 등)에 따라 유효한 공격 리스트
    private AICharacterAttackAction _selectedAttack; 
    private AICharacterAttackAction _previousAttack; 
    protected bool hasAttack = false;

    [Header("콤보 설정")] 
    private bool _canPerformCombo = false;
    [SerializeField] protected int chanceToPerformCombo = 25; // 콤보 공격 확률 (퍼센트)

    [Header("전투 거리 설정")]
    [SerializeField] public float maximumEngagementDistance = 5; // 이 거리보다 멀어지면 추적 상태로 전환

    public override AIState Tick(AICharacterManager aiCharacter)
    {
        // 조건 검사를 통한 조기 반환
        if (ShouldWaitForCurrentAction(aiCharacter))
            return this;

        if (ShouldReturnToIdle(aiCharacter))
            return SwitchState(aiCharacter, aiCharacter.stateIdle);

        if (ShouldPursueTarget(aiCharacter))
            return SwitchState(aiCharacter, aiCharacter.statePursueTarget);

        // 공격 준비 및 실행
        return HandleCombatLogic(aiCharacter);
    }

    #region Helper Methods

    protected bool ShouldWaitForCurrentAction(AICharacterManager aiCharacter)
    {
        return aiCharacter.isPerformingAction;
    }

    protected bool ShouldReturnToIdle(AICharacterManager aiCharacter)
    {
        EnsureNavMeshAgentEnabled(aiCharacter);
        RotateTowardsTarget(aiCharacter);
    
        return aiCharacter.aiCharacterCombatManager.currentTarget == null;
    }

    protected bool ShouldPursueTarget(AICharacterManager aiCharacter)
    {
        return aiCharacter.aiCharacterCombatManager.distanceFromTarget > maximumEngagementDistance;
    }

    protected virtual AIState HandleCombatLogic(AICharacterManager aiCharacter)
    {
        if (!hasAttack)
        {
            GetNewAttack(aiCharacter);
            return this; // 공격 선택 후 다음 프레임에서 실행
        }

        return ExecuteAttack(aiCharacter);
    }

    private AIState ExecuteAttack(AICharacterManager aiCharacter)
    {
        aiCharacter.stateAttack.currentAttack = _selectedAttack;
        aiCharacter.stateAttack.willPerformCombo = _canPerformCombo;
        return SwitchState(aiCharacter, aiCharacter.stateAttack);
    }

    private void EnsureNavMeshAgentEnabled(AICharacterManager aiCharacter)
    {
        if (!aiCharacter.navMeshAgent.enabled)
            aiCharacter.navMeshAgent.enabled = true;
    }

    private void RotateTowardsTarget(AICharacterManager aiCharacter)
    {
        aiCharacter.aiCharacterCombatManager.RotateTowardsAgent(aiCharacter);
    }

    #endregion

    // 현재 상황에 맞는 공격을 필터링하고 확률 기반으로 선택
    protected virtual void GetNewAttack(AICharacterManager aiCharacter)
    {
        _potentialAttacks = new List<AICharacterAttackAction>();

        foreach (var potentialAttack in aiCharacterAttacks)
        {
            // 최소 공격 거리보다 가까우면 제외
            if (potentialAttack.minimumAttackDistance > aiCharacter.aiCharacterCombatManager.distanceFromTarget)
                continue;

            // 최대 공격 거리보다 멀면 제외
            if (potentialAttack.maximumAttackDistance < aiCharacter.aiCharacterCombatManager.distanceFromTarget)
                continue;

            // 최소 시야 각도보다 작으면 제외
            if (potentialAttack.minimumAttackAngle > aiCharacter.aiCharacterCombatManager.viewableAngle)
                continue;

            // 최대 시야 각도보다 크면 제외
            if (potentialAttack.maximumAttackAngle < aiCharacter.aiCharacterCombatManager.viewableAngle) 
                continue;

            _potentialAttacks.Add(potentialAttack);
        }

        // 가능한 공격이 없으면 리턴
        if (_potentialAttacks.Count <= 0)
            return;

        // 가중치 기반 확률 공격 선택
        var totalWeight = 0;

        foreach (var attack in _potentialAttacks)
        {
            totalWeight += attack.attackWeight;
        }

        var randomWeightValue = Random.Range(1, totalWeight + 1);
        var processedWeight = 0;

        foreach (var attack in _potentialAttacks)
        {
            processedWeight += attack.attackWeight;

            if (randomWeightValue <= processedWeight)
            {
                _selectedAttack = attack;
                _previousAttack = _selectedAttack;
                hasAttack = true;
                _canPerformCombo = RollForOutcomeChance(chanceToPerformCombo);
                return;
            }
        }
    }

    // 특정 확률로 true 반환 (예: 콤보 공격 확률 체크)
    protected virtual bool RollForOutcomeChance(int outcomeChance)
    {
        bool outcomeWillBePerformed = false;

        int randomPercentage = Random.Range(0, 100);

        if (randomPercentage < outcomeChance)
            outcomeWillBePerformed = true;

        return outcomeWillBePerformed;
    }

    // 상태 전환 시 초기화할 플래그들
    protected override void ResetStateFlags(AICharacterManager aiCharacter)
    {
        hasAttack = false;
        _canPerformCombo = false;
    }
}
