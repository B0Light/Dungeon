using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Character Actions/Weapon Actions/light Attack Action")]
public class BaseAttackAction_Light : WeaponItemAction
{
    [SerializeField] private string horizontalAttack01 = "LightAttack01";
    [SerializeField] private string horizontalAttack02 = "LightAttack02";
    [SerializeField] private string horizontalAttack03 = "LightAttack03";
    
    [Header("Running Attack")]
    [SerializeField] private string runningAttack01 = "OH_Running_Attack_01";
    [SerializeField] private string rollingAttack01 = "OH_Rolling_Attack_01";
    [SerializeField] private string backStepAttack01 = "OH_BackStep_Attack_01";
    [SerializeField] private string jumpingAttack01 = "OH_Jumping_Attack_01_Start";
    
    [Header("Critical Attack")]
    [SerializeField] private string criticalAttackBack = "CriticalAttack_Back";
    [SerializeField] private string criticalAttackFront = "CriticalAttack_Front";
    public override void AttemptToPerformAction(PlayerManager playerPerformingAction, EquipmentItemInfoWeapon usedWeaponItemInfo)
    {
        base.AttemptToPerformAction(playerPerformingAction,usedWeaponItemInfo);
        
        if (!playerPerformingAction.characterVariableManager.CLVM.isGrounded)
        { 
            if (playerPerformingAction.characterCombatManager.canPerformJumpingAttack &&
                playerPerformingAction.playerVariableManager.perkJumpAttack.Value)
            {
                playerPerformingAction.characterVariableManager.isAttacking.Value = true;
                PerformJumpingAttack(playerPerformingAction, usedWeaponItemInfo);
                return;
            }
        }
        else
        {
            playerPerformingAction.characterVariableManager.isAttacking.Value = true;

            if (playerPerformingAction.characterCombatManager.canCriticalAttack)
            {
                PerformCriticalAttack(playerPerformingAction, usedWeaponItemInfo);
                return;
            }
            
            if (playerPerformingAction.characterCombatManager.canPerformRollingAttack)
            {
                PerformRollingAttack(playerPerformingAction, usedWeaponItemInfo);
                return;
            }
            
            if (playerPerformingAction.characterVariableManager.CLVM.isSprinting)
            {
                PerformRunningAttack(playerPerformingAction, usedWeaponItemInfo);
                return;
            }
            
            if (playerPerformingAction.characterCombatManager.canPerformBackStepAttack &&
                playerPerformingAction.playerVariableManager.perkBackStepAttack.Value)
            {
                PerformBackStepAttack(playerPerformingAction, usedWeaponItemInfo);
                return;
            }
    
            PerformLightAttack(playerPerformingAction, usedWeaponItemInfo);
        }

        
    }

    protected virtual void PerformLightAttack(PlayerManager playerPerformingAction, EquipmentItemInfoWeapon equipmentItemInfoWeaponPerformingAction)
    {
        if(playerPerformingAction.playerCombatManager.enableCanDoCombo &&
            playerPerformingAction.isPerformingAction)
        {
            playerPerformingAction.playerCombatManager.enableCanDoCombo = false;

            if(playerPerformingAction.characterCombatManager.lastAttackAnimationPerformed == horizontalAttack01)
            {
                playerPerformingAction.playerAnimatorManager.PlayTargetAttackActionAnimation(
                    equipmentItemInfoWeaponPerformingAction, AttackType.LightAttack02, horizontalAttack02, true, actionPoint: actionCost);
            }
            else if(playerPerformingAction.characterCombatManager.lastAttackAnimationPerformed == horizontalAttack02 &&
                    playerPerformingAction.playerVariableManager.perkThirdCombo.Value)
            {
                playerPerformingAction.playerAnimatorManager.PlayTargetAttackActionAnimation(
                    equipmentItemInfoWeaponPerformingAction, AttackType.LightAttack03, horizontalAttack03, true, actionPoint: actionCost);
            }
            else
            {
                playerPerformingAction.playerAnimatorManager.PlayTargetAttackActionAnimation(
                    equipmentItemInfoWeaponPerformingAction, AttackType.LightAttack01, horizontalAttack01, true, actionPoint: actionCost);
            }
        }
        else if(!playerPerformingAction.isPerformingAction) // BaseAttack
        {
            playerPerformingAction.playerAnimatorManager.PlayTargetAttackActionAnimation(
                equipmentItemInfoWeaponPerformingAction, AttackType.LightAttack01, horizontalAttack01, true, actionPoint: actionCost);
        }
    }

    protected virtual void PerformCriticalAttack(PlayerManager playerPerformingAction, EquipmentItemInfoWeapon equipmentItemInfoWeaponPerformingAction)
    {
        playerPerformingAction.playerCombatManager.canCriticalAttack = false;
        var victimCharacter = playerPerformingAction.playerCombatManager.criticalDamagedCharacter;
        if(victimCharacter == null) return;
        playerPerformingAction.gameObject.transform.LookAt(victimCharacter.transform);
            
        float angle = Vector3.SignedAngle(playerPerformingAction.transform.forward, victimCharacter.transform.forward, Vector3.up);

        bool isFront = angle > 90 || angle < -90;

        string attackAnimation = isFront ? criticalAttackFront : criticalAttackBack;
        string victimAnimation = isFront ? victimCharacter.characterAnimatorManager.criticalAttack_Front_Victim 
            : victimCharacter.characterAnimatorManager.criticalAttack_Back_Victim;

        playerPerformingAction.playerAnimatorManager.PlayTargetAttackActionAnimation(
            equipmentItemInfoWeaponPerformingAction, AttackType.CriticalAttack, attackAnimation, true, actionPoint: 0);

        victimCharacter.characterAnimatorManager.PlayTargetActionAnimation(victimAnimation, true);
    }
    
    protected virtual void PerformRunningAttack(PlayerManager playerPerformingAction, EquipmentItemInfoWeapon equipmentItemInfoWeaponPerformingAction)
    {
        playerPerformingAction.playerAnimatorManager.PlayTargetAttackActionAnimation(
            equipmentItemInfoWeaponPerformingAction, AttackType.RunningAttack01, runningAttack01, true, actionPoint: actionCost);
    }
    
    protected virtual void PerformRollingAttack(PlayerManager playerPerformingAction, EquipmentItemInfoWeapon equipmentItemInfoWeaponPerformingAction)
    {
        playerPerformingAction.playerCombatManager.canPerformRollingAttack = false;
        playerPerformingAction.playerVariableManager.isInvulnerable.Value = false;
        playerPerformingAction.playerAnimatorManager.PlayTargetAttackActionAnimation(
            equipmentItemInfoWeaponPerformingAction, AttackType.RollingAttack01, rollingAttack01, true, actionPoint: actionCost);
    }

    protected virtual void PerformBackStepAttack(PlayerManager playerPerformingAction, EquipmentItemInfoWeapon equipmentItemInfoWeaponPerformingAction)
    {
        playerPerformingAction.playerCombatManager.canPerformBackStepAttack = false;
        playerPerformingAction.playerAnimatorManager.PlayTargetAttackActionAnimation(
            equipmentItemInfoWeaponPerformingAction, AttackType.BackStepAttack01, backStepAttack01, true, actionPoint: actionCost);
    }
    
    protected virtual void PerformJumpingAttack(PlayerManager playerPerformingAction, EquipmentItemInfoWeapon equipmentItemInfoWeaponPerformingAction)
    {
        playerPerformingAction.playerCombatManager.canPerformJumpingAttack = false;
        playerPerformingAction.playerAnimatorManager.PlayTargetAttackActionAnimation(
            equipmentItemInfoWeaponPerformingAction, AttackType.JumpingAttack01, jumpingAttack01, true, canMove: true, actionPoint: actionCost);
    }
}
