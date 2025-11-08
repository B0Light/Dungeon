using UnityEngine;

[CreateAssetMenu(menuName = "Character Actions/Weapon Actions/heavy Attack Action")]
public class BaseAttackAction_Heavy : ScriptableObject, IWeaponItemAction
{
    [SerializeField] private float staminaCost = 15f;
    
    private readonly int _heavyAttack01 = Animator.StringToHash("HeavyAttack01");
    private readonly int _heavyAttack02 = Animator.StringToHash("HeavyAttack02");
    private readonly int _heavyAttack03 = Animator.StringToHash("HeavyAttack03");
    
    public void AttemptToPerformAction(PlayerManager player, EquipmentItemInfoWeapon weaponInfo)
    {
        if (player.playerVariableManager.stamina.Value <= staminaCost ||
            !player.characterVariableManager.CLVM.isGrounded)
            return;
        
        player.characterVariableManager.isAttacking.Value = true;

        PerformHeavyAttack(player, weaponInfo);
    }

    private void PerformHeavyAttack(PlayerManager player, EquipmentItemInfoWeapon weaponInfo)
    {
        if (player.isPerformingAction)
        {
            if (!player.playerCombatManager.enableCanDoCombo) return;
            
            player.playerCombatManager.enableCanDoCombo = false;

            if (player.characterCombatManager.lastAttackAction == _heavyAttack01)
            {
                player.playerAnimatorManager.PlayTargetAttackActionAnimation(weaponInfo, _heavyAttack02);
            }
            else if (player.characterCombatManager.lastAttackAction == _heavyAttack02 &&
                     player.playerVariableManager.perkThirdCombo.Value)
            {
                player.playerAnimatorManager.PlayTargetAttackActionAnimation(weaponInfo, _heavyAttack03);
            }
            else
            {
                player.playerAnimatorManager.PlayTargetAttackActionAnimation(weaponInfo, _heavyAttack01);
            }
        }
        else
        {
            player.playerAnimatorManager.PlayTargetAttackActionAnimation(weaponInfo, _heavyAttack01);
        }


    }
}
