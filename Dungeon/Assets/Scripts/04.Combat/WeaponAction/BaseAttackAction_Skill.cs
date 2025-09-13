using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Character Actions/Weapon Actions/skill")]
public class BaseAttackAction_Skill : WeaponItemAction
{
    [SerializeField] protected string skill = "Skill";
    
    [SerializeField] protected float cooldownTime = 60f; // 스킬 쿨타임 (초 단위)
    private bool _isCooldown = true; 
    
    public override void AttemptToPerformAction(PlayerManager playerPerformingAction, EquipmentItemInfoWeapon usedWeaponItemInfo)
    {
        base.AttemptToPerformAction(playerPerformingAction,usedWeaponItemInfo);
        
        // check for stops
        if (playerPerformingAction.playerVariableManager.actionPoint.Value <= 0)
            return;

        if (!playerPerformingAction.characterVariableManager.CLVM.isGrounded)
            return;

        PerformSkill(playerPerformingAction, usedWeaponItemInfo);
    }
    
    protected virtual void PerformSkill(PlayerManager playerPerformingAction, EquipmentItemInfoWeapon curEquippedWeaponInfo)
    {
        if (!(playerPerformingAction.isPerformingAction || _isCooldown)) return;
        
        Debug.LogWarning("USE SKILL");
        playerPerformingAction.StartCoroutine(SetCoolTime());
        playerPerformingAction.playerAnimatorManager.PlayTargetAttackActionAnimation(
            curEquippedWeaponInfo, AttackType.Skill, skill, true, actionPoint: actionCost);
        
    }
    
    private IEnumerator SetCoolTime()
    {
        _isCooldown = true;

        yield return new WaitForSeconds(cooldownTime);

        _isCooldown = false;
    }
}
