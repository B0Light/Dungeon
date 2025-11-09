using UnityEngine;

[CreateAssetMenu(menuName = "Character Actions/Weapon Actions/02.heavy Attack Action")]
public class BaseAttackAction_Heavy : ScriptableObject, IWeaponItemAction
{
    [SerializeField] private float staminaCost = 15f;
    
    private readonly int _heavyAttack01 = Animator.StringToHash("HeavyAttack01");
    private readonly int _heavyAttack02 = Animator.StringToHash("HeavyAttack02");
    private readonly int _heavyAttack03 = Animator.StringToHash("HeavyAttack03");
    
    public void AttemptToPerformAction(PlayerManager player)
    {
        if (player.playerVariableManager.stamina.Value <= staminaCost ||
            !player.characterVariableManager.CLVM.isGrounded)
            return;
        
        player.characterVariableManager.isAttacking.Value = true;

        PerformHeavyAttack(player);
    }

    private void PerformHeavyAttack(PlayerManager player)
    {
        if (player.isPerformingAction)
        {
            if (!player.playerCombatManager.enableCanDoCombo) return;
            
            player.playerCombatManager.enableCanDoCombo = false;

            if (player.characterCombatManager.lastAttackAction == _heavyAttack01)
            {
                player.playerAnimatorManager.PlayTargetAttackActionAnimation(_heavyAttack02, staminaCost);
            }
            else if (player.characterCombatManager.lastAttackAction == _heavyAttack02 &&
                     player.playerVariableManager.perkThirdCombo.Value)
            {
                player.playerAnimatorManager.PlayTargetAttackActionAnimation(_heavyAttack03, staminaCost);
            }
            else
            {
                player.playerAnimatorManager.PlayTargetAttackActionAnimation(_heavyAttack01, staminaCost);
            }
        }
        else
        {
            player.playerAnimatorManager.PlayTargetAttackActionAnimation(_heavyAttack01, staminaCost);
        }
    }
}
