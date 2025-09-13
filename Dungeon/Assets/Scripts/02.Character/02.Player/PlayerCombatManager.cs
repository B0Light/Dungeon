using UnityEngine;
using UnityEngine.Serialization;

public class PlayerCombatManager : CharacterCombatManager
{
    private PlayerManager _player;

    [HideInInspector] public EquipmentItemInfoWeapon currentEquipmentItemInfoWeaponBeingUsed;

    [HideInInspector] public bool enableCanDoCombo = false;

    protected override void Awake()
    {
        base.Awake();

        _player = GetComponent<PlayerManager>();
    }
    
    // equipableItemInfoWeaponPerformingAction 은 추후 해당 무기 애니메이션을 획득하기 위해 필요 
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
    
    public override void EnableCanDoCombo()
    {
        _player.playerCombatManager.enableCanDoCombo = true;
    }

    public override void DisableCanDoCombo()
    {
        _player.playerCombatManager.enableCanDoCombo = false;
    }
    
    

    public void SlashAttack(int attackType)
    {
        WorldCharacterEffectsManager.Instance.CastSwordSlash(transform.position, _player.playerVariableManager.currentEquippedWeaponID.Value, attackType, _player);
    }
}
