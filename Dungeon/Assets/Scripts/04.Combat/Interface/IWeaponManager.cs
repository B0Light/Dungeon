using UnityEngine;

public interface IWeaponManager
{ 
    void SetWeapon(CharacterManager characterWieldingWeapon ,EquipmentItemInfoWeapon equipmentItemInfoWeapon);
    void OpenDamageCollider();
    void CloseDamageCollider();
}
