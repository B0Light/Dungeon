using UnityEngine;
using System.Collections.Generic;

public class ItemInfo : ItemData
{
    public ItemType itemType;
    
    public int saleCost = 0;
    [HideInInspector] public bool purChaseWithItem = false;
    
    public GameObject itemModel;
    
    public List<ItemAbility> itemAbilities;
    public float weight = 0;
    
    
}
