using UnityEngine;

public class CharacterEquipmentMangaer : MonoBehaviour
{
    public enum WeaponModelSlot
    {
        RightHand,
        LeftHand,
        LeftChainsaw,
        //Right Hips,
        //Left Hips,
        //Back
    }
    
    public EquipmentItemInfoWeapon currentEquippedInfoWeapon;
    public EquipmentItemInfoHelmet currentEquippedInfoHelmet;
    public EquipmentItemInfoArmor currentEquippedInfoArmor;
    
    public virtual void CloseDamageCollider()
    {
        
    }
}
