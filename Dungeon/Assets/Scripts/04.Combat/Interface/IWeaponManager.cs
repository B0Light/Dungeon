using UnityEngine;

public interface IWeaponManager
{ 
    void SetWeapon(CharacterManager owner ,EquipmentItemInfoWeapon equipmentItemInfoWeapon);
    void OpenDamageCollider();
    void CloseDamageCollider();
}
