using UnityEngine;

public class PlayerCombatManager : CharacterCombatManager
{
    private PlayerManager _player;
    
    protected override void Awake()
    {
        base.Awake();

        _player = GetComponent<PlayerManager>();
    }
    
    public void PerformWeaponBasedAction()
    {
        
    }
    
}
