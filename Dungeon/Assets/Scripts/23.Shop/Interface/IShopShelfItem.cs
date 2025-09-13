using UnityEngine;

public interface IShopShelfItem
{
    public void Init(ItemData data);

    int GetItemCategory();

    ItemData GetItem();

    int GetItemCode();
}
