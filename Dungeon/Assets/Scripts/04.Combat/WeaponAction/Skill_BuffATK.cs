using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Character Actions/Weapon Actions/11.skill_buffATK")]
public class Skill_BuffATK : BaseAttackAction_Skill
{
    [SerializeField] private float buffTime = 60f;
    
    protected override void ActiveSkill(PlayerManager player)
    {
        player.StartCoroutine(GetEffect(player));
    }

    private IEnumerator GetEffect(PlayerManager playerPerformingAction)
    {
        playerPerformingAction.playerStatsManager.extraDamage.Value += 100;
        yield return new WaitForSeconds(buffTime);
        playerPerformingAction.playerStatsManager.extraDamage.Value -= 100;
    }
}
