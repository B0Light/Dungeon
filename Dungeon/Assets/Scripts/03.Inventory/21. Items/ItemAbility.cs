using UnityEngine.Serialization;

[System.Serializable]
public class ItemAbility
{
    public ItemEffect itemEffect;
    public int value;
    
    public ItemAbility(ItemEffect itemEffect, int value)
    {
        this.itemEffect = itemEffect;
        this.value = value;
    }
}
