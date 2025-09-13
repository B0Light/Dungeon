using UnityEngine;

public class InteractableItem : Interactable
{
    [SerializeField] private int itemID;
    public void SetItemCode(int itemCode)
    {
        itemID = itemCode;
        interactableText = "Get " + WorldDatabase_Item.Instance.GetItemByID(itemCode).itemName;
    }
    
    public override void Interact(PlayerManager player)
    {
        base.Interact(player);
        // Player inventory가 아닌 world shopManager를 통해서 추가하는 이유는
        // inventory에 아이템으로 추가될 ui 정보를 world shop Manager가 가지고 있기때문에
        // world shop Manager를 통해서 처리한다. 
        if (WorldShopManager.Instance.BuyItem(WorldDatabase_Item.Instance.GetItemByID(itemID)))
        {
            Destroy(transform.root.gameObject);
        }
        else
        {
            ResetInteraction();
        }
    }

    public int GetItemCode() => itemID;
}
