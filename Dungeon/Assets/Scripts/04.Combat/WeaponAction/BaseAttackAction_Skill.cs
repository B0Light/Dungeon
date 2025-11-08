using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Character Actions/Weapon Actions/skill")]
public class BaseAttackAction_Skill : IWeaponItemAction
{
    private readonly int _skill = Animator.StringToHash("skill");
    
    protected float cooldownTime = 60f; // 스킬 쿨타임 (초 단위)
    private bool _isCooldown = true; 
    
    public void AttemptToPerformAction(PlayerManager player, EquipmentItemInfoWeapon usedWeaponItemInfo)
    {
        // check for stops
        if (player.playerVariableManager.stamina.Value <= 0)
            return;

        if (!player.characterVariableManager.CLVM.isGrounded)
            return;

        PerformSkill(player, usedWeaponItemInfo);
    }
    
    private void PerformSkill(PlayerManager player, EquipmentItemInfoWeapon weaponInfo)
    {
        if (!(player.isPerformingAction || _isCooldown)) return;
        
        Debug.LogWarning("USE SKILL");
        PerformSkill(player);
        player.StartCoroutine(SetCoolTime());
        player.playerAnimatorManager.PlayTargetAttackActionAnimation(weaponInfo, _skill);
    }
    
    private IEnumerator SetCoolTime()
    {
        _isCooldown = true;

        yield return new WaitForSeconds(cooldownTime);

        _isCooldown = false;
    }

    protected virtual void PerformSkill(PlayerManager player){}
}
