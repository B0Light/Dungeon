
using UnityEngine;

public class PlayerStatsManager : CharacterStatsManager
{
    private PlayerManager _player;
    
    protected override void Awake()
    {
        base.Awake();

        _player = character as PlayerManager;
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

