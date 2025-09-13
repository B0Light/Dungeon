using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class InventoryController : MonoBehaviour
{
    [HideInInspector] public bool isActive = false;

    [SerializeField] private Transform mainInventoryCanvas;
    
    private InventoryHighlight _inventoryHighlight;
    private Vector2Int _oldPosition;
    private InventoryItem _itemToHighlight;
    
    private InventoryItem _overlapItem;

    [SerializeField] private List<ItemInfo> items;
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private Transform canvasTransform;
    
    private ItemGrid _selectedItemGrid;
    
    private ItemGrid _lastSelectedItemGrid;
    private Vector2Int _lastTileGridPosition;
    public ItemGrid SelectedItemGrid
    {
        get => _selectedItemGrid;
        set 
        {
            if (_selectedItemGrid != value)
            {
                _selectedItemGrid = value; 
                _inventoryHighlight.SetParent(value);
            }
        }
    }
    
    private InventoryItem _selectedItem;
    public event Action<InventoryItem> OnSelectedItemChanged;
    public InventoryItem SelectedItem
    {
        get => _selectedItem;
        set
        {
            SetSelectedItemState(_selectedItem, false);

            _selectedItem = value;
            SetSelectedItemState(_selectedItem, true);

            if (_selectedItem != null)
            {
                UpdateHighlight();
            }
            else
            {
                _inventoryHighlight.ShowSelector(false);
            }
        }
    }

    private void SetSelectedItemState(InventoryItem item, bool isSelected)
    {
        if (item == null) return;

        item.gameObject.GetComponent<Image>().raycastTarget = !isSelected;
    }

    private void UpdateHighlight()
    {
        _inventoryHighlight.SetPosition(_selectedItemGrid, _itemToHighlight);
        _inventoryHighlight.ShowSelector(true);
        _lastSelectedItemGrid = _selectedItemGrid;
    }
    
    
    private void Awake() {
        _inventoryHighlight = GetComponent<InventoryHighlight>();
        isActive = false;
    }
    
    private void Update()
    {
        if (!isActive)
        {
            HandleHighlight();
            if (Input.GetMouseButtonDown(0))
            {
                SelectItemOnMousePos();
            }
        }
        else
        {
            if (SelectedItem)
            {
                SelectedItem.gameObject.transform.SetParent(mainInventoryCanvas);
                SelectedItem.gameObject.transform.position = Input.mousePosition;
            }
            
        
            if (_selectedItemGrid == null) {
                _inventoryHighlight.ShowHighlighter(false);
            }
        
            HandleHighlight();
        }
    }

    public void RotateItem()
    {
        if(SelectedItem == null) return;
        SelectedItem.Rotate();
    }
    
    private void HandleHighlight()
    {
        if (_selectedItemGrid == null)
        {
            _inventoryHighlight.ShowHighlighter(false);
            GUIController.Instance.inventoryGUIManager.CloseItemToolTip();
            return;
        }
        Vector2Int positionOnGrid = GetTileGridPosition();
        if (SelectedItem == null)
        {
            _itemToHighlight = _selectedItemGrid.GetItem(positionOnGrid.x,positionOnGrid.y);
            // 현재 선택된 아이템이 없는데 내가 있는 마우스 포지션에 선택할 수 있는 아이템이 있음 
            if(_itemToHighlight!= null) 
            {
                _inventoryHighlight.ShowHighlighter(true);
                _inventoryHighlight.SetSize(_itemToHighlight);
                _inventoryHighlight.SetPosition(_selectedItemGrid, _itemToHighlight);
                _inventoryHighlight.SetSelectorParent(_selectedItemGrid);
                
                GUIController.Instance.inventoryGUIManager.SetItemToolTip(_itemToHighlight.itemInfoData);
            }
            // 현재 선택된 아이템이 없는데 내가 있는 마우스 포지션이 빈칸임 
            else
            {
                _inventoryHighlight.ShowHighlighter(false);
                
                GUIController.Instance.inventoryGUIManager.CloseItemToolTip();
            }
        }
        else
        { 
            GUIController.Instance.inventoryGUIManager.CloseItemToolTip();
            //물건을 잡았을 때 이벤토리에 넣을 수 없으면 highlighter을 숨긴다.
            _inventoryHighlight.ShowHighlighter(_selectedItemGrid.BoundaryCheck(
                positionOnGrid.x,
                positionOnGrid.y,
                SelectedItem.Width,
                SelectedItem.Height
            ));
            // 물체가 회전한경우 아아템의 도착지점의 하이라이트도 회전해야함 -> 실제 회전이 아니라 width, high 값 변경
            _inventoryHighlight.SetSize(SelectedItem, false);
            _inventoryHighlight.SetPosition(_selectedItemGrid, SelectedItem,positionOnGrid.x,positionOnGrid.y);
        }
    }

    private void SelectItemOnMousePos()
    {
        if (SelectedItemGrid)
        {
            var tileGridPosition = GetTileGridPosition();
            InventoryItem selectedItem = _selectedItemGrid.SelectItemOnGridPos(tileGridPosition.x, tileGridPosition.y);
            if(selectedItem)
            {
                OnSelectedItemChanged?.Invoke(selectedItem);
            }
        }
    }

    public void LeftMouseButtonPress()
    {
        if (SelectedItemGrid)
        {
            _lastTileGridPosition = GetTileGridPosition();
            if (SelectedItem)
            {
                PlaceItem(_lastTileGridPosition);
            }
            else
            {
                PickUpItem(_lastTileGridPosition);
            }
        }
        else
        {
            if (SelectedItem)
            {
                DiscardItem();
            }
        }
    }

    public void RightMouseButtonPress()
    {
        if (_selectedItemGrid == null) return;
        if (SelectedItem)
        {
            ResetSelectedItem();
            return;
        }

        if (_itemToHighlight == null) return;

        GUIController.Instance.inventoryGUIManager.CloseItemToolTip();
        _lastTileGridPosition = GetTileGridPosition();

        var backpack = WorldPlayerInventory.Instance.GetBackpackInventory();
        var inventory = WorldPlayerInventory.Instance.GetInventory();
        var targetGrid = WorldPlayerInventory.Instance.curInteractItemGrid;
        var opened = WorldPlayerInventory.Instance.curOpenedInventory;
        
        InventoryItem pickUpItem = _selectedItemGrid.PickUpItem(_lastTileGridPosition.x, _lastTileGridPosition.y);
        if(pickUpItem == null) return;
        switch (_selectedItemGrid.itemGridType)
        {
            case ItemGridType.PlayerInventory:
            case ItemGridType.BackpackInventory:
                // 1. 외부 인벤토리로 이동
                if (opened == ItemGridType.InteractableInventory || opened == ItemGridType.ShareInventory)
                {
                    if (!targetGrid.AddItem(pickUpItem.gameObject, false))
                    {
                        _selectedItemGrid.AddItem(pickUpItem.gameObject);
                    }
                    break;
                }

                // 2. 내부 사용
                pickUpItem = _selectedItemGrid.PickUpItem(pickUpItem);
                if (_selectedItemGrid.UseItem(pickUpItem)) break;
                // 3. PlayerInventory <-> Backpack 간 이동

                
                if (opened == ItemGridType.BackpackInventory)
                {
                    pickUpItem = _selectedItemGrid.PickUpItem(pickUpItem);
                    
                    if (_selectedItemGrid.itemGridType == ItemGridType.BackpackInventory)
                    {
                        if (!inventory.AddItem(pickUpItem.gameObject, false))
                        {
                            _selectedItemGrid.AddItem(pickUpItem.gameObject);
                        }
                    }
                    else if (_selectedItemGrid.itemGridType == ItemGridType.PlayerInventory)
                    {
                        if (!backpack.AddItem(pickUpItem.gameObject, false))
                        {
                            _selectedItemGrid.AddItem(pickUpItem.gameObject);
                        }
                    }
                }
                break;
                
            case ItemGridType.InteractableInventory:
            case ItemGridType.ShareInventory:
                if (!inventory.AddItem(pickUpItem.gameObject, false) && !backpack.AddItem(pickUpItem.gameObject, false))
                {
                    _selectedItemGrid.AddItem(pickUpItem.gameObject, false);
                }
                break;
            case ItemGridType.EquipmentInventory:
                if (!backpack.AddItem(pickUpItem.gameObject, false))
                {
                    if (!inventory.AddItem(pickUpItem.gameObject, false))
                    {
                        _selectedItemGrid.AddItem(pickUpItem.gameObject, false);
                    }
                }
                break;

            default:
                break;
        }
    }



    private Vector2Int GetTileGridPosition()
    {
        Vector2 position = Input.mousePosition;
        if (SelectedItem != null)
        {
            position.x -= (SelectedItem.Width - 1) * ItemGrid.TileSizeWidth / 2;
            position.y += (SelectedItem.Height - 1) * ItemGrid.TileSizeHeight / 2;
        }
        return _selectedItemGrid.GetTileGridPosition(position);
    }

    private void PickUpItem(Vector2Int tileGridPosition)
    {
        SelectedItem = _selectedItemGrid.PickUpItem(tileGridPosition.x, tileGridPosition.y);
    }
    private void PlaceItem(Vector2Int tileGridPosition)
    {
        if(_selectedItemGrid.PlaceItem(SelectedItem, tileGridPosition.x, tileGridPosition.y, false))
            SelectedItem = null;
    }

    public Transform GetItemOnPointerTransform()
    {
        if (_selectedItemGrid == null) return null;
        Vector2Int positionOnGrid = GetTileGridPosition();
        InventoryItem inventoryItem = _selectedItemGrid.GetItem(positionOnGrid.x,positionOnGrid.y);

        return inventoryItem ? inventoryItem.transform : null;
    }

    public void ResetSelectedItem()
    {
        if(!SelectedItem) return;   
        
        if (_lastSelectedItemGrid.PlaceItem(SelectedItem, SelectedItem.onGridPositionX, SelectedItem.onGridPositionY, false))
        {
            //Destroy(SelectedItem.gameObject);
            _lastSelectedItemGrid = null;
            SelectedItem = null;
        }
        else
        {
            Debug.LogWarning("Something wrong");
        }
    }

    private void DiscardItem()
    {
        if(!SelectedItem) return;

        // 땅에 떨어뜨릴 아이템 생성 
        Vector3 spawnPos = GameManager.Instance.GetPlayer().transform.position + new Vector3(0,1,0.5f); 
        GameObject item = Instantiate(WorldDatabase_Item.Instance.emptyInteractItemPrefab, spawnPos, quaternion.identity);
        InteractableItem interactableItem = item.GetComponentInChildren<InteractableItem>();
        interactableItem.SetItemCode(SelectedItem.itemInfoData.itemCode);
        
        // 손에 들고 있는 아이템 제거 
        Destroy(SelectedItem.gameObject);
        _lastSelectedItemGrid = null;
        SelectedItem = null;
    }
}