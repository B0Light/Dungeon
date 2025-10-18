using UnityEngine;

public class PlayerCombatManager : CharacterCombatManager
{
    private PlayerManager _player;
    
    protected override void Awake()
    {
        base.Awake();

        _player = GetComponent<PlayerManager>();
    }

    [ContextMenu("OnCombat")]
    public void OnCombat()
    {
        InputHandlerManager.Instance.SetControlActive(false);
    }
    
    public void PerformWeaponBasedAction()
    {
        
    }
    
}
