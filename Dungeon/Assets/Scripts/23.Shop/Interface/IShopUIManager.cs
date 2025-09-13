using System.Collections.Generic;

public interface IShopUIManager
{
    void OpenShop(List<int> itemIds, Interactable interactable = null);

    void SelectItemToBuy(ItemData selectItem);

    void SearchCategory(int itemType);

    void ShowAllItem();

}
