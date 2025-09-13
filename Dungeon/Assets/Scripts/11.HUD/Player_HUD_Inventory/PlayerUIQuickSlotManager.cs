using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIQuickSlotManager : MonoBehaviour
{
    [Header("QUICK SLOT - Consumable")] 
    private Dictionary<int, List<GameObject>> _quickSlotItemDict = new Dictionary<int, List<GameObject>>();
    private bool _quickSlotIsOpen = false;
    [SerializeField] private Image selectQuickSlotItemIcon;
    [SerializeField] private Transform consumableItemSlot;
    [SerializeField] private GameObject itemSlotPrefab;
    
    public void ToggleQuickSlotItem()
    {
        _quickSlotIsOpen = !_quickSlotIsOpen;
        SetActiveQuickSlot(_quickSlotIsOpen);
    }
    
    public void AddQuickSlotItem(int itemID)
    {
        ItemInfoConsumable infoConsumableItemInfo = (ItemInfoConsumable)WorldDatabase_Item.Instance.GetItemByID(itemID);
        
        if (infoConsumableItemInfo == null && infoConsumableItemInfo.itemIcon)
            return;

        GameObject spawnSlotObject = Instantiate(itemSlotPrefab, consumableItemSlot);
        AddGameObject(itemID,spawnSlotObject);
        UI_QuickSlotItem spawnSlot = spawnSlotObject.GetComponent<UI_QuickSlotItem>();
        spawnSlot.SetItem(infoConsumableItemInfo);
        spawnSlotObject.SetActive(_quickSlotIsOpen);
    }
    
    private void AddGameObject(int key, GameObject gameObjectToAdd)
    {
        if (!_quickSlotItemDict.ContainsKey(key))
        {
            _quickSlotItemDict[key] = new List<GameObject>();
        }
        
        _quickSlotItemDict[key].Add(gameObjectToAdd);
    }

    public void RemoveQuickSlotItem(int itemID)
    {
        // 현재 퀵슬롯에 등록된 아이템에 해당 아이템이 있는지 확인 
        if (!_quickSlotItemDict.TryGetValue(itemID, out var items)) return;
        // 있는데 해당 리스트가 비었다면 (다쓴 상태) 퀵슬롯의 키 값에서 제거   
        if (items.Count == 0)
        {
            _quickSlotItemDict.Remove(itemID);
            return;
        }
        
        GameObject objToRemove = items[0];
        items.RemoveAt(0); 
        Destroy(objToRemove.gameObject);

        if (items.Count == 0)
        {
            _quickSlotItemDict.Remove(itemID);
        }
    }

    public void ClearQuickSlot()
    {
        foreach (KeyValuePair<int, List<GameObject>> entry in _quickSlotItemDict)
        {
            foreach (GameObject obj in entry.Value)
            {
                if (obj != null)
                {
                    Destroy(obj);
                }
            }
        }
        _quickSlotItemDict.Clear();
    }

    public void ChangeQuickSlotItem(int itemID)
    {
        SetQuickSlotItem(itemID);
    }

    private void SetActiveQuickSlot(bool active)
    {
        foreach (KeyValuePair<int, List<GameObject>> entry in _quickSlotItemDict)
        {
            foreach (GameObject obj in entry.Value)
            {
                if (obj != null)
                {
                    obj.SetActive(active);  
                }
            }
        }
    }
    private void SetQuickSlotItem(int itemID)
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
