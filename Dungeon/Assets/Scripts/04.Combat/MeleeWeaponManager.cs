using UnityEngine;

public class MeleeWeaponManager : MonoBehaviour, IWeaponManager
{
    private MeleeWeaponDamageCollider _meleeDamageCollider;
    private ParticleSystem _weaponTrail;

    private EquipmentItemInfoWeapon _weaponInfo;
    private CharacterManager _owner;

    private void Awake()
    {
        _meleeDamageCollider = GetComponentInChildren<MeleeWeaponDamageCollider>();
        _weaponTrail = GetComponentInChildren<ParticleSystem>();
    }

    public void SetWeapon(CharacterManager characterWieldingWeapon ,EquipmentItemInfoWeapon equipmentItemInfoWeapon)
    {
        _weaponInfo = equipmentItemInfoWeapon;
        _owner = characterWieldingWeapon;
        
        Debug.Log("Set Weapon : " + equipmentItemInfoWeapon.itemName);
        _meleeDamageCollider.SetWeaponDamage(characterWieldingWeapon, equipmentItemInfoWeapon);
    }
    
    public void OpenDamageCollider()
    {
        GUIController.Instance.playerUIHudManager.playerUIWeaponSlotManager.GlowWeaponSlot();
        _meleeDamageCollider.EnableDamageCollider();
        _weaponTrail?.Play();
    }
    
    public void CloseDamageCollider()
    {
        _meleeDamageCollider.DisableDamageCollider();
        _weaponTrail?.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }   
}
