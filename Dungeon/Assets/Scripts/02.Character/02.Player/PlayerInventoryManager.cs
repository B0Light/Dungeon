using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerInventoryManager : CharacterInventoryManager
{
    public EquipmentItemInfoWeapon currentEquippedInfoWeapon;
    public EquipmentItemInfoHelmet currentEquippedInfoHelmet;
    public EquipmentItemInfoArmor currentEquippedInfoArmor;
}
