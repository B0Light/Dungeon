using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_QuickSlotItem : MonoBehaviour
{
    [SerializeField] private Image itemIcon;

    public void SetItem(ItemInfo itemInfoInfo)
    {
        itemIcon.sprite = itemInfoInfo.itemIcon;
    }
}
