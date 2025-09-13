using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "A.I/States/Attack")]
public class AttackState : AIState
{
    [Header("Current Attack")]
    [HideInInspector] public AICharacterAttackAction currentAttack;
    [HideInInspector] public bool willPerformCombo = false;

    [Header("State Flags")] 
    private bool _hasPerformedAttack = false;
    private bool _hasPerformedCombo = false;

    [Header("Pivot After Attack")] 
    [SerializeField] protected bool pivotAfterAttack = false;

    public override AIState Tick(AICharacterManager aiCharacter)
    {
        if (aiCharacter.aiCharacterCombatManager.currentTarget == null)
            return SwitchState(aiCharacter, aiCharacter.stateIdle);
        
        if(aiCharacter.aiCharacterCombatManager.currentTarget.isDead.Value)
            return SwitchState(aiCharacter, aiCharacter.stateIdle);
        
        aiCharacter.aiCharacterCombatManager.RotateTowardsTargetWhilstAttacking(aiCharacter);
        
        if (willPerformCombo && !_hasPerformedCombo && !_hasPerformedAttack)
        {
            if (currentAttack.comboAction != null)
            {
                _hasPerformedCombo = true;
                currentAttack.comboAction.AttemptToPerformAction(aiCharacter);
            }
        }
        
        if (aiCharacter.isPerformingAction)
            return this;

        if (!aiCharacter.isActionRecover)
        {
            aiCharacter.StartActionRecovery(currentAttack.actionRecoverTime);
            return this;
        }
        
        if (!_hasPerformedAttack)
        {
            PerformAttack(aiCharacter);
            return this;
        }

        return SwitchState(aiCharacter, aiCharacter.stateCombatStance);
    }

    private void PerformAttack(AICharacterManager aiCharacter)
    {
        _hasPerformedAttack = true;
        currentAttack.AttemptToPerformAction(aiCharacter);
        aiCharacter.StartActionRecovery(currentAttack.actionRecoverTime);
    }

    protected override void ResetStateFlags(AICharacterManager aiCharacter)
    {
        _hasPerformedAttack = false;
        _hasPerformedCombo = false;
    }
}
    