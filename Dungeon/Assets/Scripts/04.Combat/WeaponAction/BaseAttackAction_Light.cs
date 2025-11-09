using UnityEngine;

[CreateAssetMenu(menuName = "Character Actions/Weapon Actions/01.light Attack Action")]
public class BaseAttackAction_Light : ScriptableObject, IWeaponItemAction
{
    [SerializeField] private float staminaCost = 10f;
    
    private readonly int _lightAttack01 = Animator.StringToHash("LightAttack01");
    private readonly int _lightAttack02 = Animator.StringToHash("LightAttack02");
    private readonly int _lightAttack03 = Animator.StringToHash("LightAttack03");
    
    private readonly int _runningAttack = Animator.StringToHash("RunningAttack");
    private readonly int _rollingAttack = Animator.StringToHash("RollingAttack");
    private readonly int _backStepAttack = Animator.StringToHash("BackSteppingAttack");
    private readonly int _jumpingAttack = Animator.StringToHash("JumpingAttack");
    
    private readonly int _criticalAttack = Animator.StringToHash("CriticalAttack");
    
    public void AttemptToPerformAction(PlayerManager player)
    {
        if (!player.characterVariableManager.CLVM.isGrounded)
        { 
            if (!player.characterVariableManager.isAttacking.Value &&
                player.characterCombatManager.canPerformJumpingAttack &&
                player.playerVariableManager.perkJumpAttack.Value)
            {
                player.characterVariableManager.isAttacking.Value = true;
                PerformJumpingAttack(player);
            }
        }
        else
        {
            player.characterVariableManager.isAttacking.Value = true;

            if (player.characterCombatManager.canCriticalAttack)
            {
                PerformCriticalAttack(player);
                return;
            }
            
            if (player.characterCombatManager.canPerformRollingAttack)
            {
                PerformRollingAttack(player);
                return;
            }
            
            if (player.characterVariableManager.CLVM.isSprinting)
            {
                PerformRunningAttack(player);
                return;
            }
            
            if (player.characterCombatManager.canPerformBackStepAttack &&
                player.playerVariableManager.perkBackStepAttack.Value)
            {
                PerformBackStepAttack(player);
                return;
            }
    
            PerformLightAttack(player);
        }
    }
    
    protected virtual void PerformLightAttack(PlayerManager player)
    {
        if(player.isPerformingAction)
        {
            if(!player.playerCombatManager.enableCanDoCombo) return;
            
            player.playerCombatManager.enableCanDoCombo = false;

            if(player.characterCombatManager.lastAttackAction == _lightAttack01)
                player.playerAnimatorManager.PlayTargetAttackActionAnimation(_lightAttack02, staminaCost);
            
            else if(player.characterCombatManager.lastAttackAction == _lightAttack02 &&
                    player.playerVariableManager.perkThirdCombo.Value)
                player.playerAnimatorManager.PlayTargetAttackActionAnimation(_lightAttack03, staminaCost);
            
            else
                player.playerAnimatorManager.PlayTargetAttackActionAnimation(_lightAttack01, staminaCost);
        }
        else
            player.playerAnimatorManager.PlayTargetAttackActionAnimation(_lightAttack01, staminaCost);
    }

    protected virtual void PerformCriticalAttack(PlayerManager player)
    {
        player.playerCombatManager.canCriticalAttack = false;
        var victimCharacter = player.playerCombatManager.criticalDamagedCharacter;
        if(victimCharacter == null) return;
        player.gameObject.transform.LookAt(victimCharacter.transform);
            
        float angle = Vector3.SignedAngle(player.transform.forward, victimCharacter.transform.forward, Vector3.up);

        bool isFront = angle > 90 || angle < -90;

        int victimAnimation = isFront ? victimCharacter.characterAnimatorManager.criticalFrontVictim 
            : victimCharacter.characterAnimatorManager.criticalBackVictim;

        player.playerAnimatorManager.PlayTargetAttackActionAnimation(_criticalAttack);
        victimCharacter.characterAnimatorManager.PlayTargetAttackActionAnimation(victimAnimation, 0);
    }
    
    protected virtual void PerformRunningAttack(PlayerManager player)
    {
        player.playerAnimatorManager.PlayTargetAttackActionAnimation(_runningAttack, staminaCost);
    }
    
    protected virtual void PerformRollingAttack(PlayerManager player)
    {
        player.playerCombatManager.canPerformRollingAttack = false;
        player.playerVariableManager.isInvulnerable.Value = false;
        player.playerAnimatorManager.PlayTargetAttackActionAnimation(_rollingAttack, staminaCost);
    }

    protected virtual void PerformBackStepAttack(PlayerManager player)
    {
        player.playerCombatManager.canPerformBackStepAttack = false;
        player.playerAnimatorManager.PlayTargetAttackActionAnimation(_backStepAttack, staminaCost);
    }
    
    protected virtual void PerformJumpingAttack(PlayerManager player)
    {
        player.playerCombatManager.canPerformJumpingAttack = false;
        player.playerAnimatorManager.PlayTargetAttackActionAnimation(_jumpingAttack, staminaCost*2);
    }
}
