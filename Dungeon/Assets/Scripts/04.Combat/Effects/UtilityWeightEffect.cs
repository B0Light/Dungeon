using System.Collections;
using UnityEngine;

public class UtilityWeightEffect : IInstantCharacterEffect
{
    private float _weightReduction;
    private float _duration;
    private float _originalWeight;

    public void SetWeightReduction(float weightReduction, float duration = 0f)
    {
        _weightReduction = weightReduction;
        _duration = duration;
    }

    public override void ProcessEffect(CharacterManager effectTarget)
    {
        if (effectTarget.isDead.Value) return;

        if (effectTarget is PlayerManager playerManager)
        {
            ApplyWeightReduction(playerManager);
            
            if (_duration > 0)
            {
                playerManager.StartCoroutine(RemoveEffectAfterDuration(playerManager));
            }
        }
    }

    private void ApplyWeightReduction(PlayerManager player)
    {
        // 현재 무게를 저장하고 무게 감소 적용
        _originalWeight = player.playerVariableManager.playerWeight;
        player.playerVariableManager.playerWeight = Mathf.Max(0f, player.playerVariableManager.playerWeight - _weightReduction);
        
        // 무게에 따른 이동 계수 재계산
        player.playerVariableManager.CalculateWeightCoefficient();
        
        Debug.Log($"무게 감소 효과 적용: -{_weightReduction} (현재 무게: {player.playerVariableManager.playerWeight})");
    }

    private IEnumerator RemoveEffectAfterDuration(PlayerManager player)
    {
        yield return new WaitForSeconds(_duration);
        
        if (player != null && !player.isDead.Value)
        {
            // 원래 무게로 복원
            player.playerVariableManager.playerWeight = _originalWeight;
            
            // 무게에 따른 이동 계수 재계산
            player.playerVariableManager.CalculateWeightCoefficient();
            
            Debug.Log($"무게 감소 효과 해제: 원래 무게로 복원 ({_originalWeight})");
        }
    }
} 