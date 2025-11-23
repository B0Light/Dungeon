using UnityEngine;
using UnityEngine.UI;

public class PlayerUIQuickSlotManager : MonoBehaviour
{
    [SerializeField] private Image selectQuickSlotItemIcon;
    
    public void SetQuickSlotItem(int itemID)
    {
        if (itemID == 0)
        {
            selectQuickSlotItemIcon.gameObject.SetActive(false);
            return;
        }
        selectQuickSlotItemIcon.gameObject.SetActive(true);
        selectQuickSlotItemIcon.sprite = WorldDatabase_Item.Instance.GetItemByID(itemID).itemIcon;
    }
}
