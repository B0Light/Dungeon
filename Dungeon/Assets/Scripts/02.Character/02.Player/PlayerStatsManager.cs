
using UnityEngine;

public class PlayerStatsManager : CharacterStatsManager
{
    private PlayerManager _player;
    
    protected override void Awake()
    {
        base.Awake();

        _player = character as PlayerManager;
    }

    // InstantCharacterEffect를 통해 배고픔 수치 조정
    public void OnHungryLevelChange(int amount)
    {
        // 배고픔이 변동하면 자동으로 호출 
        _player.playerVariableManager.isHungry.Value = amount < 20;
        GUIController.Instance.playerUIHudManager.playerUIStatusManager.SetHungryLevel(amount);
        
        GUIController.Instance.playerUIHudManager.playerUIStatusManager.SetWarningHungryLevel(
            _player.playerVariableManager.isHungry.Value);
        
        if (_player.playerVariableManager.isHungry.Value)
        {
            _player.playerVariableManager.moveCoefficientByHungry = 0.5f;
        }
    }
    
    public void SetNewHealthPoint(int value)
    {
        GUIController.Instance.playerUIHudManager.playerUIStatusManager.SetNewHealthValue(value);
    }

    public void SetNewActionPoint(int value)
    {
        GUIController.Instance.playerUIHudManager.playerUIStatusManager.SetNewActionPoint(value);
    }
}

