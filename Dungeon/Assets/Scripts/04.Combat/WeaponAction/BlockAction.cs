using UnityEngine;

[CreateAssetMenu(menuName = "Character Actions/Weapon Actions/03.Block Action")]
public class BlockAction : ScriptableObject, IWeaponItemAction
{
    [SerializeField] private float staminaCost = 5f;
    
    private readonly int _block = Animator.StringToHash("Block");
    
    private bool SpendCost(PlayerManager player)
    {
        return player.playerStatsManager.UseStamina(staminaCost);
    }
    
    public void AttemptToPerformAction(PlayerManager player)
    {
        if(!SpendCost(player)) return;
        
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
        
        player.playerAnimatorManager.PlayTargetActionAnimation(_block, canMove: true, canRotate: true);
        
    }
}
