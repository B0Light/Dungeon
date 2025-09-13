using System.Collections;
using UnityEngine;

public class BuffDefenseEffect : IInstantCharacterEffect
{
    private float _physicalAbsorptionBuff;
    private float _magicalAbsorptionBuff;
    private float _duration;

    public void SetDefenseBuff(float physicalBuff, float magicalBuff, float duration = 0f)
    {
        _physicalAbsorptionBuff = physicalBuff;
        _magicalAbsorptionBuff = magicalBuff;
        _duration = duration;
    }

    public override void ProcessEffect(CharacterManager effectTarget)
    {
        if (effectTarget.isDead.Value) return;

        if (effectTarget is PlayerManager playerManager)
        {
            ApplyDefenseBuff(playerManager);
            
            if (_duration > 0)
            {
                playerManager.StartCoroutine(RemoveBuffAfterDuration(playerManager));
            }
        }
    }

    private void ApplyDefenseBuff(PlayerManager player)
    {
        player.playerStatsManager.basePhysicalAbsorption += _physicalAbsorptionBuff;
        player.playerStatsManager.baseMagicalAbsorption += _magicalAbsorptionBuff;
        Debug.Log($"방어력 버프 적용: 물리 +{_physicalAbsorptionBuff}%, 마법 +{_magicalAbsorptionBuff}%");
    }

    private IEnumerator RemoveBuffAfterDuration(PlayerManager player)
    {
        yield return new WaitForSeconds(_duration);
        
        if (player != null && !player.isDead.Value)
        {
            player.playerStatsManager.basePhysicalAbsorption -= _physicalAbsorptionBuff;
            player.playerStatsManager.baseMagicalAbsorption -= _magicalAbsorptionBuff;
            Debug.Log($"방어력 버프 해제: 물리 -{_physicalAbsorptionBuff}%, 마법 -{_magicalAbsorptionBuff}%");
        }
    }
} 