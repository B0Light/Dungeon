
public class PlayerCombatManager : CharacterCombatManager
{
    private PlayerManager _player;
    
    protected override void Awake()
    {
        base.Awake();

        _player = GetComponent<PlayerManager>();
    }
    
    public void PerformWeaponBasedAction(AttackType actionType)
    {
        if(_player.playerEquipmentManger.currentEquippedInfoWeapon == null ||
           _player.playerEquipmentManger.currentEquippedInfoWeapon.itemCode == 0) return;
        IWeaponItemAction weaponItemAction;
        switch (actionType)
        {
            case AttackType.LightAttack01:
                weaponItemAction = lightAttackAction;
                break;
            case AttackType.HeavyAttack01:
            case AttackType.ChargeAttack01:
                weaponItemAction = heavyAttackAction;
                break;
            case AttackType.Parry:
            case AttackType.Block:
                weaponItemAction = blockAction;
                break;
            default:
                weaponItemAction = lightAttackAction;
                break;
        }
        weaponItemAction.AttemptToPerformAction(_player);
    }
}
