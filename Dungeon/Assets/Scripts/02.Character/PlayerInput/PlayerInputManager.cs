using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PlayerInputManager : Singleton<PlayerInputManager>
{
    // LOCAL PLAYER
    private PlayerManager _playerManager;

    private PlayerControls _playerControls;
    
    public float buttonHoldThreshold = 0.15f;
    
    public Vector2 moveComposite;

    public float movementInputDuration;
    public bool movementInputDetected;

    private bool _isSprinting = false;
    private CharacterLocomotionVariableManager CLVM => _playerManager.characterVariableManager.CLVM;

    public void SetPlayer(PlayerManager playerManager)
    {
        _playerManager = playerManager;
    }

    private void OnEnable()
    {
        if(_playerControls == null)
        {
            _playerControls = new PlayerControls();

            // Locomotion
            _playerControls.PlayerLocomotion.Move.performed += OnMovePerformed;
            _playerControls.PlayerLocomotion.Move.canceled += OnMoveCanceled;
            _playerControls.PlayerLocomotion.Jump.performed += OnJumpPerformed;
            _playerControls.PlayerLocomotion.ToggleWalk.performed += OnToggleWalkPerformed;
            _playerControls.PlayerLocomotion.Sprint.performed += OnSprintPerformed;
            _playerControls.PlayerLocomotion.Sprint.canceled += OnSprintCanceled;
            _playerControls.PlayerLocomotion.ToggleCrouch.performed += OnToggleCrouchPerformed;
            _playerControls.PlayerLocomotion.LockOn.performed += OnLockOnPerformed;

            // Player Actions
            _playerControls.PlayerActions.Interact.performed += OnInteractPerformed;
            _playerControls.PlayerActions.Roll.performed += OnRollPerformed;
            

            // Inventory Actions
            _playerControls.PlayerInventory.ToggleInventory.performed += OnToggleInventoryPerformed;
            _playerControls.PlayerInventory.ToggleQuickSlot.performed += OnToggleQuickSlotPerformed;
            _playerControls.PlayerInventory.SwitchLQuickSlot.performed += OnSwitchLQuickSlotPerformed;
            _playerControls.PlayerInventory.SwitchRQuickSlot.performed += OnSwitchRQuickSlotPerformed;
            _playerControls.PlayerInventory.UseQuickSlotItem.performed += OnUseQuickSlotPerformed;
            
            // Menu Actions
            _playerControls.UI.ToggleOption.performed += OnEscapePerformed;
            _playerControls.UI.NextGUI.performed += OnNextGUIPerformed;
            _playerControls.UI.OpenMap.performed += OnOpenMapPerformed;
            _playerControls.UI.Click.performed += OnGUIClickPerformed;
            _playerControls.UI.DoubleClick.performed += OnGUIDoubleClickPerformed;
            _playerControls.UI.RightClick.performed += OnGUIRightClickPerformed;
            _playerControls.UI.Rotate.performed += OnGUIRotatePerformed;
        }

        _playerControls.Enable();
    }

    private void OnDisable()
    {
        // 구독 해제
        if (_playerControls != null)
        {
            _playerControls.PlayerLocomotion.Move.performed -= OnMovePerformed;
            _playerControls.PlayerLocomotion.Move.canceled -= OnMoveCanceled;
            _playerControls.PlayerLocomotion.Jump.performed -= OnJumpPerformed;
            _playerControls.PlayerLocomotion.ToggleWalk.performed -= OnToggleWalkPerformed;
            _playerControls.PlayerLocomotion.Sprint.performed -= OnSprintPerformed;
            _playerControls.PlayerLocomotion.Sprint.canceled -= OnSprintCanceled;
            _playerControls.PlayerLocomotion.ToggleCrouch.performed -= OnToggleCrouchPerformed;
            _playerControls.PlayerLocomotion.LockOn.performed -= OnLockOnPerformed;

            _playerControls.PlayerActions.Interact.performed -= OnInteractPerformed;
            _playerControls.PlayerActions.Roll.performed -= OnRollPerformed;
            
            _playerControls.PlayerInventory.ToggleInventory.performed -= OnToggleInventoryPerformed;
            _playerControls.PlayerInventory.ToggleQuickSlot.performed -= OnToggleQuickSlotPerformed;
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
        }

        _playerControls?.Disable();
    }

    private void OnApplicationFocus(bool focus)
    {
        if(!enabled) return;
        
        if(focus)
        {
            _playerControls.Enable();
        }
        else
        {
            _playerControls.Disable();
        }
        
    }

    private void Update()
    {
        // Update 함수는 연속적인 입력 값 처리에 사용됩니다.
        HandleContinuousInput();
    }
    
    private void HandleContinuousInput()
    {
        // 이동 입력 감지는 Update에서 처리
        movementInputDetected = moveComposite.magnitude > 0;
        if (movementInputDetected)
        {
            movementInputDuration += Time.deltaTime;
        }
        else
        {
            movementInputDuration = 0;
        }
    }

    //
    // 이벤트 콜백 함수들
    //
    
    // Locomotion
    private void OnMovePerformed(InputAction.CallbackContext context) => moveComposite = context.ReadValue<Vector2>();
    private void OnMoveCanceled(InputAction.CallbackContext context) => moveComposite = Vector2.zero;

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        if (_playerManager.playerVariableManager.canControl.Value)
            _playerManager.playerLocomotionManager.AttemptToJump();
    }
    
    private void OnToggleWalkPerformed(InputAction.CallbackContext context)
    {
        if (_playerManager.playerVariableManager.canControl.Value)
            _playerManager.playerLocomotionManager.AttemptToToggleWalk();
    }
    
    private void OnSprintPerformed(InputAction.CallbackContext context)
    {
        if (_playerManager.playerVariableManager.canControl.Value)
        {
            if (_isSprinting) return;
            _isSprinting = true;
            _playerManager.playerLocomotionManager.DeactivateCrouch();
            _playerManager.playerLocomotionManager.AttemptToActivateSprint();
        }
    }
    
    private void OnSprintCanceled(InputAction.CallbackContext context)
    {
        if (_isSprinting)
        {
            _isSprinting = false;
            _playerManager.playerLocomotionManager.AttemptToDeactivateSprint();
        }
    }
    
    private void OnToggleCrouchPerformed(InputAction.CallbackContext context)
    {
        if (_playerManager.playerVariableManager.canControl.Value)
        {
            _playerManager.playerLocomotionManager.AttemptToToggleCrouch();
            _isSprinting = false;
        }
    }
    
    private void OnLockOnPerformed(InputAction.CallbackContext context)
    {
        if (_playerManager.playerVariableManager.canControl.Value)
        {
            _playerManager.playerLocomotionManager.AttemptToLockOn();
            _playerManager.playerLocomotionManager.AttemptToDeactivateSprint();
        }
    }

    // Player Actions
    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        if (_playerManager.playerVariableManager.canControl.Value)
            _playerManager.playerInteractionManager.Interact();
    }
    
    private void OnRollPerformed(InputAction.CallbackContext context)
    {
        if (_playerManager.playerVariableManager.canControl.Value)
            _playerManager.playerLocomotionManager.AttemptToRoll();
    }
    

    // Inventory
    private void OnToggleInventoryPerformed(InputAction.CallbackContext context)
    {
        GUIController.Instance.HandleTab();
    }

    private void OnToggleQuickSlotPerformed(InputAction.CallbackContext context)
    {
        if (_playerManager.playerVariableManager.canControl.Value)
            GUIController.Instance.playerUIHudManager.playerUIQuickSlotManager.ToggleQuickSlotItem();
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

    public void SetControlActive(bool isActive)
    {
        if (isActive)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            
            if(_playerManager != null)
                _playerManager.playerVariableManager.canControl.Value = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            if(_playerManager != null)
                _playerManager.playerVariableManager.canControl.Value = false;
        }
    }
    
    public void CalculateInput()
    {
        Vector3 moveDirection = Vector3.zero;
        if (movementInputDetected)
        {
            CLVM.movementInputTapped = movementInputDuration == 0;
            CLVM.movementInputPressed = movementInputDuration > 0 && movementInputDuration < buttonHoldThreshold;
            CLVM.movementInputHeld = movementInputDuration >= buttonHoldThreshold;
            
            moveDirection = (PlayerCameraController.Instance.GetCameraForwardZeroedYNormalized() * moveComposite.y) +
                            (PlayerCameraController.Instance.GetCameraRightZeroedYNormalized() * moveComposite.x);
        }
        else
        {
            CLVM.movementInputTapped = false;
            CLVM.movementInputPressed = false;
            CLVM.movementInputHeld = false;
        }
        CLVM.moveDirection = moveDirection;
    }
}