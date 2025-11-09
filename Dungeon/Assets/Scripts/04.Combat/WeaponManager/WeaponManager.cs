using UnityEngine;

public class WeaponManager : MonoBehaviour, IWeaponManager
{
    private DamageCollider _damageCollider;
    
    private void Awake()
    {
        _damageCollider = GetComponentInChildren<DamageCollider>();
    }
    
    public void SetWeapon(CharacterManager owner ,EquipmentItemInfoWeapon equipmentItemInfoWeapon)
    {
        _damageCollider.SetWeaponDamage(owner, equipmentItemInfoWeapon);
        Debug.Log("Set Weapon : " + equipmentItemInfoWeapon.itemName);
    }
    
    public void OpenDamageCollider()
    {
        _damageCollider.EnableDamageCollider();
    }
    
    public void CloseDamageCollider()
    {
        _damageCollider.DisableDamageCollider();
    }   
    
}
