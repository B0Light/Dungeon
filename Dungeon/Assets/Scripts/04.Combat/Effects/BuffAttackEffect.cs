using System.Collections;
using UnityEngine;

public class BuffAttackEffect : IInstantCharacterEffect
{
    private int _buffAmount;
    private float _duration;

    public void SetBuffAmount(int buffAmount, float duration = 0f)
    {
        _buffAmount = buffAmount;
        _duration = duration;
    }

    public override void ProcessEffect(CharacterManager effectTarget)
    {
        if (effectTarget.isDead.Value) return;

        if (effectTarget is PlayerManager playerManager)
        {
            ApplyAttackBuff(playerManager);
            
            if (_duration > 0)
            {
                playerManager.StartCoroutine(RemoveBuffAfterDuration(playerManager));
            }
        }
    }

    private void ApplyAttackBuff(PlayerManager player)
    {
        player.playerStatsManager.extraDamage.Value += _buffAmount;
        Debug.Log($"공격력 버프 적용: +{_buffAmount}");
    }

    private IEnumerator RemoveBuffAfterDuration(PlayerManager player)
    {
        yield return new WaitForSeconds(_duration);
        
        if (player != null && !player.isDead.Value)
        {
            player.playerStatsManager.extraDamage.Value -= _buffAmount;
            Debug.Log($"공격력 버프 해제: -{_buffAmount}");
        }
    }
} 