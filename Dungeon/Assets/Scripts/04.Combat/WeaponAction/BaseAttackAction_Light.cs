using UnityEngine;

[CreateAssetMenu(menuName = "Character Actions/Weapon Actions/light Attack Action")]
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
    
    public void AttemptToPerformAction(PlayerManager player, EquipmentItemInfoWeapon weaponInfo)
    {
        if (!player.characterVariableManager.CLVM.isGrounded)
        { 
            if (!player.characterVariableManager.isAttacking.Value &&
                player.characterCombatManager.canPerformJumpingAttack &&
                player.playerVariableManager.perkJumpAttack.Value)
            {
                player.characterVariableManager.isAttacking.Value = true;
                PerformJumpingAttack(player, weaponInfo);
            }
        }
        else
        {
            player.characterVariableManager.isAttacking.Value = true;

            if (player.characterCombatManager.canCriticalAttack)
            {
                PerformCriticalAttack(player, weaponInfo);
                return;
            }
            
            if (player.characterCombatManager.canPerformRollingAttack)
            {
                PerformRollingAttack(player, weaponInfo);
                return;
            }
            
            if (player.characterVariableManager.CLVM.isSprinting)
            {
                PerformRunningAttack(player, weaponInfo);
                return;
            }
            
            if (player.characterCombatManager.canPerformBackStepAttack &&
                player.playerVariableManager.perkBackStepAttack.Value)
            {
                PerformBackStepAttack(player, weaponInfo);
                return;
            }
    
            PerformLightAttack(player, weaponInfo);
        }

        
    }

    protected virtual void PerformLightAttack(PlayerManager player, EquipmentItemInfoWeapon weaponInfo)
    {
        if(player.isPerformingAction)
        {
            if(!player.playerCombatManager.enableCanDoCombo) return;
            
            player.playerCombatManager.enableCanDoCombo = false;

            if(player.characterCombatManager.lastAttackAction == _lightAttack01)
                player.playerAnimatorManager.PlayTargetAttackActionAnimation(weaponInfo, _lightAttack01);
            
            else if(player.characterCombatManager.lastAttackAction == _lightAttack02 &&
                    player.playerVariableManager.perkThirdCombo.Value)
                player.playerAnimatorManager.PlayTargetAttackActionAnimation(weaponInfo, _lightAttack03);
            
            else
                player.playerAnimatorManager.PlayTargetAttackActionAnimation(weaponInfo, _lightAttack01);
        }
        else
            player.playerAnimatorManager.PlayTargetAttackActionAnimation(weaponInfo, _lightAttack01);
    }

    protected virtual void PerformCriticalAttack(PlayerManager player, EquipmentItemInfoWeapon weaponInfo)
    {
        player.playerCombatManager.canCriticalAttack = false;
        var victimCharacter = player.playerCombatManager.criticalDamagedCharacter;
        if(victimCharacter == null) return;
        player.gameObject.transform.LookAt(victimCharacter.transform);
            
        float angle = Vector3.SignedAngle(player.transform.forward, victimCharacter.transform.forward, Vector3.up);

        bool isFront = angle > 90 || angle < -90;

        int victimAnimation = isFront ? victimCharacter.characterAnimatorManager.criticalFrontVictim 
            : victimCharacter.characterAnimatorManager.criticalBackVictim;

        player.playerAnimatorManager.PlayTargetAttackActionAnimation(weaponInfo, _criticalAttack);
        victimCharacter.characterAnimatorManager.PlayTargetActionAnimation(victimAnimation, true);
    }
    
    protected virtual void PerformRunningAttack(PlayerManager player, EquipmentItemInfoWeapon weaponInfo)
    {
        player.playerAnimatorManager.PlayTargetAttackActionAnimation(weaponInfo, _runningAttack);
    }
    
    protected virtual void PerformRollingAttack(PlayerManager player, EquipmentItemInfoWeapon weaponInfo)
    {
        player.playerCombatManager.canPerformRollingAttack = false;
        player.playerVariableManager.isInvulnerable.Value = false;
        player.playerAnimatorManager.PlayTargetAttackActionAnimation(weaponInfo, _rollingAttack);
    }

    protected virtual void PerformBackStepAttack(PlayerManager player, EquipmentItemInfoWeapon weaponInfo)
    {
        player.playerCombatManager.canPerformBackStepAttack = false;
        player.playerAnimatorManager.PlayTargetAttackActionAnimation(weaponInfo, _backStepAttack);
    }
    
    protected virtual void PerformJumpingAttack(PlayerManager player, EquipmentItemInfoWeapon weaponInfo)
    {
        player.playerCombatManager.canPerformJumpingAttack = false;
        player.playerAnimatorManager.PlayTargetAttackActionAnimation(weaponInfo, _jumpingAttack);
    }
}
