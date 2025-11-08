using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Character Actions/Weapon Actions/Block Action")]
public class BlockAction : IWeaponItemAction
{
    private readonly int _block = Animator.StringToHash("Block");
    
    public void AttemptToPerformAction(PlayerManager player, EquipmentItemInfoWeapon usedWeaponItemInfo)
    {
        if (player.playerVariableManager.stamina.Value < usedWeaponItemInfo.baseActionCost)
        {
            Debug.Log("No ActionPoint");
            return;
        }
        
        if (!player.characterVariableManager.CLVM.isGrounded)
        {
            Debug.Log("On Air");
            return;
        }

        if (player.playerVariableManager.isAttacking.Value)
        {
            Debug.Log("On Attack");
            return;
        }
        
        if(player.isPerformingAction) return;
        
        
        if (player.playerVariableManager.isBlock.Value)
        {
            Debug.Log("Already Block");
            return;
        }
        
        player.playerAnimatorManager.PlayTargetAttackActionAnimation(
            usedWeaponItemInfo, _block, canMove: true, canRotate: true);
        
    }
}
