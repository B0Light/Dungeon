using System.Collections;
using UnityEngine;

public class UtilitySpeedEffect : IInstantCharacterEffect
{
    private float _speedMultiplier;
    private float _duration;
    private float _originalMoveCoefficient;

    public void SetSpeedBuff(float speedMultiplier, float duration = 0f)
    {
        _speedMultiplier = speedMultiplier;
        _duration = duration;
    }

    public override void ProcessEffect(CharacterManager effectTarget)
    {
        if (effectTarget.isDead.Value) return;

        if (effectTarget is PlayerManager playerManager)
        {
            ApplySpeedBuff(playerManager);
            
            if (_duration > 0)
            {
                playerManager.StartCoroutine(RemoveBuffAfterDuration(playerManager));
            }
        }
    }

    private void ApplySpeedBuff(PlayerManager player)
    {
        // 현재 이동 계수를 저장하고 속도 버프 적용
        _originalMoveCoefficient = player.playerVariableManager.moveCoefficientByWeight;
        player.playerVariableManager.moveCoefficientByWeight *= _speedMultiplier;
        
        // 최대 속도 제한 (2.0배까지)
        player.playerVariableManager.moveCoefficientByWeight = Mathf.Min(player.playerVariableManager.moveCoefficientByWeight, 2.0f);
        
        Debug.Log($"속도 버프 적용: {_speedMultiplier}배 (현재 계수: {player.playerVariableManager.moveCoefficientByWeight})");
    }

    private IEnumerator RemoveBuffAfterDuration(PlayerManager player)
    {
        yield return new WaitForSeconds(_duration);
        
        if (player != null && !player.isDead.Value)
        {
            // 원래 계수로 복원
            player.playerVariableManager.moveCoefficientByWeight = _originalMoveCoefficient;
            
            // 무게에 따른 이동 계수 재계산
            player.playerVariableManager.CalculateWeightCoefficient();
            
            Debug.Log($"속도 버프 해제: 원래 속도로 복원");
        }
    }
} 