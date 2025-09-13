using UnityEngine;

[CreateAssetMenu(menuName = "Character Actions/Weapon Actions/heavy Attack Action")]
public class BaseAttackAction_Heavy : WeaponItemAction
{
    [SerializeField] private string verticalAttack01 = "HeavyAttack01";
    [SerializeField] private string verticalAttack02 = "HeavyAttack02";
    [SerializeField] private string verticalAttack03 = "HeavyAttack03";
    public override void AttemptToPerformAction(PlayerManager playerPerformingAction, EquipmentItemInfoWeapon usedWeaponItemInfo)
    {
        base.AttemptToPerformAction(playerPerformingAction,usedWeaponItemInfo);
        
        // check for stops
        if (playerPerformingAction.playerVariableManager.actionPoint.Value <= 0)
            return;

        if (!playerPerformingAction.characterVariableManager.CLVM.isGrounded)
            return;
        
        playerPerformingAction.characterVariableManager.isAttacking.Value = true;

        PerformHeavyAttack(playerPerformingAction, usedWeaponItemInfo);
    }

    private void PerformHeavyAttack(PlayerManager playerPerformingAction, EquipmentItemInfoWeapon equipmentItemInfoWeaponPerformingAction)
    {
        if (playerPerformingAction.playerCombatManager.enableCanDoCombo &&
            playerPerformingAction.isPerformingAction)
        {
            playerPerformingAction.playerCombatManager.enableCanDoCombo = false;

            if (playerPerformingAction.characterCombatManager.lastAttackAnimationPerformed == verticalAttack01)
            {
                playerPerformingAction.playerAnimatorManager.PlayTargetAttackActionAnimation(equipmentItemInfoWeaponPerformingAction, AttackType.HeavyAttack02, verticalAttack02, true);
            }
            else if (playerPerformingAction.characterCombatManager.lastAttackAnimationPerformed == verticalAttack02 &&
                     playerPerformingAction.playerVariableManager.perkThirdCombo.Value)
            {
                playerPerformingAction.playerAnimatorManager.PlayTargetAttackActionAnimation(equipmentItemInfoWeaponPerformingAction, AttackType.HeavyAttack03, verticalAttack03, true);
            }
            else
            {
                playerPerformingAction.playerAnimatorManager.PlayTargetAttackActionAnimation(equipmentItemInfoWeaponPerformingAction, AttackType.HeavyAttack01, verticalAttack01, true);
            }
        }
        else if (!playerPerformingAction.isPerformingAction) // BaseAttack
        {
            playerPerformingAction.playerAnimatorManager.PlayTargetAttackActionAnimation(equipmentItemInfoWeaponPerformingAction, AttackType.HeavyAttack01, verticalAttack01, true);
        }


    }
}
