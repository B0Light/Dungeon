using UnityEngine;

public class PlayerCombatManager : CharacterCombatManager
{
    private PlayerManager _player;
    
    [HideInInspector] public bool enableCanDoCombo = false;

    protected override void Awake()
    {
        base.Awake();

        _player = GetComponent<PlayerManager>();
    }
    
    public void PerformWeaponBasedAction(WeaponItemAction weaponAction, EquipmentItemInfoWeapon equipmentItemInfoWeaponPerformingAction)
    {
        if (weaponAction && equipmentItemInfoWeaponPerformingAction)
        {
            if (character.characterVariableManager.actionPoint.Value >= weaponAction.actionCost)
            {
                weaponAction.AttemptToPerformAction(_player, equipmentItemInfoWeaponPerformingAction);
            }
        }
        else
        {
            Debug.Log("No Weapon Action");
        }
    }
    
    public void PerformWeaponDirAction(WeaponItemAction weaponAction, EquipmentItemInfoWeapon equipmentItemInfoWeaponPerformingAction, Dir dir)
    {
        if (weaponAction && equipmentItemInfoWeaponPerformingAction)
        {
            weaponAction.AttemptToPerformAction(_player, equipmentItemInfoWeaponPerformingAction);
        }
        else
        {
            Debug.Log("No Weapon Action");
        }
    }
    
    public override void EnableCanDoCombo()
    {
        _player.playerCombatManager.enableCanDoCombo = true;
    }

    public override void DisableCanDoCombo()
    {
        _player.playerCombatManager.enableCanDoCombo = false;
    }
}
