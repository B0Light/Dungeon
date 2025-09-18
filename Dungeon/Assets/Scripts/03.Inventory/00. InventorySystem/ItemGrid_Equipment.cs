
using System.Collections.Generic;
using UnityEngine;

public class ItemGrid_Equipment : ItemGrid
{
    [SerializeField] private PlayerManager _playerManager;
    
    [SerializeField] private ItemType itemType;
    private List<InventoryItem> _curEquipItem = new List<InventoryItem>();

    public override bool CheckPlaceItem(InventoryItem inventoryItem, int posX, int posY)
    {
        if (base.CheckPlaceItem(inventoryItem, posX, posY))
        {
            if (itemType != inventoryItem.itemInfoData.itemType) return false;
            return true;
        }

        return false;
    }

    public override bool PlaceItem(InventoryItem inventoryItem, int posX, int posY, bool isLoad = false)
    {
        if (itemType != ItemType.Consumables && _curEquipItem.Count > 0) return false;
        
        if (base.PlaceItem(inventoryItem, posX, posY, isLoad))
        {
            if (!GetPlayerManager())
            {
                Debug.LogError("NO PLAYER MANAGER");
            }
            _curEquipItem.Add(inventoryItem);
            switch (itemType)   
            {
                case ItemType.Weapon:
                    _playerManager.playerVariableManager.currentEquippedWeaponID.Value = inventoryItem.itemInfoData.itemCode;
                    break;
                case ItemType.Armor:
                    _playerManager.playerVariableManager.currentArmorID.Value = inventoryItem.itemInfoData.itemCode;
                    break;
                case ItemType.Helmet:
                    _playerManager.playerVariableManager.currentHelmetID.Value = inventoryItem.itemInfoData.itemCode;
                    break;
                case ItemType.Consumables:
                    _playerManager.playerVariableManager.currentQuickSlotIDList.Add(inventoryItem.itemInfoData.itemCode);
                    break;
            }
            return true;
        }
        
        return false;
    }
    
    protected override void PlaceItemsAuto(List<int> setItemList, bool isLoad = true)
    {
        foreach (var itemCode in setItemList)
        {
            if (AddItemById(itemCode, isLoad:isLoad)) continue;
            
            // 새로 아이템 추가에 실패 -> 아이템 드롭
            // 플레이어가 슬롯 특전 반환하며 아이템 슬롯이 줄어들 경우 수행할 것으로 예상 
            Vector3 spawnPos = GameManager.Instance.GetPlayer().transform.position + new Vector3(0,1,0.5f); 
            GameObject item = Instantiate(WorldDatabase_Item.Instance.emptyInteractItemPrefab, spawnPos, Quaternion.identity);
            InteractableItem interactableItem = item.GetComponentInChildren<InteractableItem>();
            interactableItem.SetItemCode(itemCode);
            
            switch (itemType)   
            {
                case ItemType.Weapon:
                    _playerManager.playerVariableManager.currentEquippedWeaponID.Value = 0;
                    break;
                case ItemType.Armor:
                    _playerManager.playerVariableManager.currentArmorID.Value = 0;
                    break;
                case ItemType.Helmet:
                    _playerManager.playerVariableManager.currentHelmetID.Value = 0;
                    break;
            }
        }
    }

    public override InventoryItem PickUpItem(int x, int y)
    {
        InventoryItem pickUpItem = base.PickUpItem(x, y);
        if (pickUpItem == null) return null;
        
        if (!GetPlayerManager())
        {
            Debug.LogError("NO PLAYER MANAGER");
        }
        
        switch (itemType)   
        {
            case ItemType.Weapon:
                _playerManager.playerVariableManager.currentEquippedWeaponID.Value = 0;
                break;
            case ItemType.Helmet:
                _playerManager.playerVariableManager.currentHelmetID.Value = 0;
                break;
            case ItemType.Armor:
                _playerManager.playerVariableManager.currentArmorID.Value = 0;
                break;
            case ItemType.Consumables:
                _playerManager.playerVariableManager.currentQuickSlotIDList.Remove(pickUpItem.itemInfoData.itemCode);
                break;
        }
        _curEquipItem.Remove(pickUpItem);
        return pickUpItem;
    }
    
    public override void ResetItemGrid()
    {
        // 리스트를 복사한 후 반복문을 돌면서 안전하게 제거
        var itemsToRemove = new List<InventoryItem>(_curEquipItem);

        foreach (var inventoryItem in itemsToRemove)
        {
            _curEquipItem.Remove(inventoryItem); // 리스트에서 제거
            RemoveItem(inventoryItem);   // 게임 오브젝트 삭제
        }
    }


    public void RemoveItemAtGrid(int itemID)
    {
        foreach (var inventoryItem in _curEquipItem)
        {
            if (inventoryItem.itemInfoData.itemCode != itemID) continue;
            
            _curEquipItem.Remove(inventoryItem);
            RemoveItem(inventoryItem);
            return;
        }
    }

    private bool GetPlayerManager()
    {
        return _playerManager == null ?
            _playerManager = GameManager.Instance.GetPlayer() :
            _playerManager;
    }
}
