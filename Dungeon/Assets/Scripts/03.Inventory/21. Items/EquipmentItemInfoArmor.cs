using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Item/Armor")]
public class EquipmentItemInfoArmor : EquipmentItemInfo
{
    [Header("Armor")]
    public int extraHealth = 10;

    [Header("Backpack")] 
    public Vector2Int backpackSize;
}
