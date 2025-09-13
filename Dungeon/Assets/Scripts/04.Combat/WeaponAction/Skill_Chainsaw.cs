using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Character Actions/Weapon Actions/skill_chainsaw")]
public class Skill_Chainsaw : BaseAttackAction_Skill
{
    [SerializeField] private float buffTime = 60f;
    
    protected override void PerformSkill(PlayerManager playerPerformingAction,
        EquipmentItemInfoWeapon curEquippedWeaponInfo)
    {
        playerPerformingAction.StartCoroutine(GetEffect(playerPerformingAction));
        base.PerformSkill(playerPerformingAction, curEquippedWeaponInfo);
    }

    private IEnumerator GetEffect(PlayerManager playerPerformingAction)
    {
        playerPerformingAction.playerStatsManager.extraDamage.Value += 100;
        yield return new WaitForSeconds(buffTime);
        playerPerformingAction.playerStatsManager.extraDamage.Value -= 100;
    }
}
