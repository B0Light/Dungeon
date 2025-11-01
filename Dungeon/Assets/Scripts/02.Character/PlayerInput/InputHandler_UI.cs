using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler_UI : MonoBehaviour, InputHandlerManager.IInputHandler
{
    private PlayerManager _playerManager;
    private PlayerControls _playerControls;
    
    private void Awake()
    {
        _playerControls = new PlayerControls();
    }

    private void Start()
    {
        InputHandlerManager.Instance.RegisterAndEnableHandler(this);
    }
    
    public void SetPlayer(PlayerManager playerManager)
    {
        _playerManager = playerManager;
    }

    public void EnableInput()
    {
        _playerControls.PlayerInventory.Enable();
        _playerControls.PlayerInventory.ToggleInventory.performed += OnToggleInventoryPerformed;
        _playerControls.PlayerInventory.SwitchLQuickSlot.performed += OnSwitchLQuickSlotPerformed;
        _playerControls.PlayerInventory.SwitchRQuickSlot.performed += OnSwitchRQuickSlotPerformed;
        _playerControls.PlayerInventory.UseQuickSlotItem.performed += OnUseQuickSlotPerformed;
            
        // Menu Actions
        _playerControls.UI.Enable();
        _playerControls.UI.ToggleOption.performed += OnEscapePerformed;
        _playerControls.UI.NextGUI.performed += OnNextGUIPerformed;
        _playerControls.UI.OpenMap.performed += OnOpenMapPerformed;
        _playerControls.UI.Click.performed += OnGUIClickPerformed;
        _playerControls.UI.DoubleClick.performed += OnGUIDoubleClickPerformed;
        _playerControls.UI.RightClick.performed += OnGUIRightClickPerformed;
        _playerControls.UI.Rotate.performed += OnGUIRotatePerformed;
    }

    public void DisableInput()
    {
        _playerControls.PlayerInventory.ToggleInventory.performed -= OnToggleInventoryPerformed;
        _playerControls.PlayerInventory.SwitchLQuickSlot.performed -= OnSwitchLQuickSlotPerformed;
        _playerControls.PlayerInventory.SwitchRQuickSlot.performed -= OnSwitchRQuickSlotPerformed;
        _playerControls.PlayerInventory.UseQuickSlotItem.performed -= OnUseQuickSlotPerformed;
            
        _playerControls.UI.ToggleOption.performed -= OnEscapePerformed;
        _playerControls.UI.NextGUI.performed -= OnNextGUIPerformed;
        _playerControls.UI.OpenMap.performed -= OnOpenMapPerformed;
        _playerControls.UI.Click.performed -= OnGUIClickPerformed;
        _playerControls.UI.DoubleClick.performed -= OnGUIDoubleClickPerformed;
        _playerControls.UI.RightClick.performed -= OnGUIRightClickPerformed;
        _playerControls.UI.Rotate.performed -= OnGUIRotatePerformed;
        
        _playerControls.PlayerInventory.Disable();
        _playerControls.UI.Disable();
    }
    
    // Inventory
    private void OnToggleInventoryPerformed(InputAction.CallbackContext context)
    {
        GUIController.Instance.HandleTab();
    }
    
    private void OnSwitchLQuickSlotPerformed(InputAction.CallbackContext context)
    {
        if (_playerManager.playerVariableManager.canControl.Value)
            SelectNextQuickSlotItem(FindCurrentSelectQuickSlotItem(), false);
    }
    
    private void OnSwitchRQuickSlotPerformed(InputAction.CallbackContext context)
    {
        if (_playerManager.playerVariableManager.canControl.Value)
            SelectNextQuickSlotItem(FindCurrentSelectQuickSlotItem(), true);
    }
    
    private void OnUseQuickSlotPerformed(InputAction.CallbackContext context)
    {
        if (_playerManager.playerVariableManager.canControl.Value)
            _playerManager.playerItemConsumeManager.UseQuickSlotItem();
    }
    
    private int FindCurrentSelectQuickSlotItem()
    {
        int index = 0;
        foreach (var itemID in _playerManager.playerVariableManager.currentQuickSlotIDList.Value)
        {
            if (_playerManager.playerVariableManager.currentSelectQuickSlotItem.Value == itemID)
            {
                return index;
            }
            index++;
        }
        return 0;
    }

    private void SelectNextQuickSlotItem(int curIndex, bool isRight)
    {
        int maxCount = _playerManager.playerVariableManager.currentQuickSlotIDList.Count;
        for (int i = 0; i < maxCount; i++)
        {
            int searchIndex = (isRight ? (i + curIndex) : (curIndex - i + maxCount)) % maxCount;

            if (_playerManager.playerVariableManager.currentSelectQuickSlotItem.Value !=
                _playerManager.playerVariableManager.currentQuickSlotIDList[searchIndex])
            {
                _playerManager.playerVariableManager.currentSelectQuickSlotItem.Value =
                    _playerManager.playerVariableManager.currentQuickSlotIDList[searchIndex];
                return;
            }
        }
    }

    // GUI
    private void OnEscapePerformed(InputAction.CallbackContext context) => GUIController.Instance.HandleEscape();
    private void OnNextGUIPerformed(InputAction.CallbackContext context) => GUIController.Instance.HandleNextGUI();
    private void OnOpenMapPerformed(InputAction.CallbackContext context) => GUIController.Instance.OpenMap();
    private void OnGUIClickPerformed(InputAction.CallbackContext context)
    {
        var inventoryController = GetInventoryController();
        if (inventoryController != null && inventoryController.isActive)
            inventoryController.LeftMouseButtonPress();
    }
    
    private void OnGUIDoubleClickPerformed(InputAction.CallbackContext context)
    {
        var inventoryController = GetInventoryController();
        if (inventoryController != null && inventoryController.isActive)
            inventoryController.RightMouseButtonPress();
    }
    
    private void OnGUIRightClickPerformed(InputAction.CallbackContext context)
    {
        var inventoryController = GetInventoryController();
        if (inventoryController != null && inventoryController.isActive)
            inventoryController.RightMouseButtonPress();
    }
    
    private void OnGUIRotatePerformed(InputAction.CallbackContext context)
    {
        var inventoryController = GetInventoryController();
        if (inventoryController != null && inventoryController.isActive)
            inventoryController.RotateItem();
    }
    
    private InventoryController GetInventoryController()
    {
        if (GUIController.Instance != null && GUIController.Instance.inventoryGUIManager != null &&
            GUIController.Instance.inventoryGUIManager.inventoryController != null)
        {
            return GUIController.Instance.inventoryGUIManager.inventoryController;
        }
        return null;
    }
    
    private void OnDestroy()
    {
        if (InputHandlerManager.Instance != null)
        {
            InputHandlerManager.Instance.UnregisterAndDisableHandler(this);
        }
    }
}
