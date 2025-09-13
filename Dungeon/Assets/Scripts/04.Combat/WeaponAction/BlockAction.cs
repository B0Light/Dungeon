using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Character Actions/Weapon Actions/Block Action")]
public class BlockAction : WeaponItemAction
{
    [SerializeField] private string block = "Block";
    public override void AttemptToPerformAction(PlayerManager playerPerformingAction, EquipmentItemInfoWeapon usedWeaponItemInfo)
    {
        base.AttemptToPerformAction(playerPerformingAction,usedWeaponItemInfo);
        
        if (playerPerformingAction.playerVariableManager.actionPoint.Value < usedWeaponItemInfo.baseActionCost)
        {
            Debug.Log("No ActionPoint");
            return;
        }
        
        if (!playerPerformingAction.characterVariableManager.CLVM.isGrounded)
        {
            Debug.Log("On Air");
            return;
        }

        if (playerPerformingAction.playerVariableManager.isAttacking.Value)
        {
            Debug.Log("On Attack");
            return;
        }
        
        if(playerPerformingAction.isPerformingAction) return;
        
        
        if (playerPerformingAction.playerVariableManager.isBlock.Value)
        {
            Debug.Log("Already Block");
            return;
        }
        
        playerPerformingAction.playerAnimatorManager.PlayTargetAttackActionAnimation(
            usedWeaponItemInfo, AttackType.Block, block, true, canMove: true, canRotate: true, actionPoint: 0);
        
    }
}
