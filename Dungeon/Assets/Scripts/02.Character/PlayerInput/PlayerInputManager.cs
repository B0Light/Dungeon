 using System;
 using Unity.Collections;
 using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class PlayerInputManager : Singleton<PlayerInputManager>
{
    // LOCAL PLAYER
    [HideInInspector]public PlayerManager playerManager;

    private PlayerControls _playerControls;
    
    public float buttonHoldThreshold = 0.15f;
    
    private CharacterLocomotionVariableManager CLVM => playerManager.characterVariableManager.CLVM;
    
    [Header("DEBUG CAMERA MOVEMENT INPUT")]
    public float cameraVerticalInput;
    public float cameraHorizontalInput;
    
    public Vector2 mouseDelta;
    public Vector2 moveComposite;

    public float movementInputDuration;
    public bool movementInputDetected;

    private bool _isSprinting = false;
    
    [ReadOnly] private bool _jumpInput = false;
    [ReadOnly] private bool _walkInput = false;
    [ReadOnly] private bool _sprintInput = false;
    [ReadOnly] private bool _crouchInput = false;
    [ReadOnly] private bool _lockOnInput = false;
    
    [ReadOnly] private bool _rollInput = false;
    
    [ReadOnly] private bool _interactionInput = false;
    
    [ReadOnly] private bool _lightAttackInput = false;
    [ReadOnly] private bool _heavyAttackInput = false;
    [ReadOnly] private bool _chargeAttackInput = false;
    
    [ReadOnly] private bool _parryInput = false;
    [ReadOnly] private bool _blockInput = false;
    
    [ReadOnly] private bool _skillInput = false;
    
    [Header("Inventory")] 
    [ReadOnly] private bool _toggleInventory = false;
    [ReadOnly] private bool _toggleQuickSlot = false;
    
    [ReadOnly] private bool _switchLQuickSlot = false;
    [ReadOnly] private bool _switchRQuickSlot = false;
    
    [ReadOnly] private bool _useQuickSlot = false;

    [Header("GUI")] 
    [ReadOnly] private bool _escapeInput = false;
    [ReadOnly] private bool _guiInput = false;
    [ReadOnly] private bool _openMapInput = false; 
    [ReadOnly] private bool _guiClickInput = false; 
    [ReadOnly] private bool _guiDoubleClickInput = false;
    [ReadOnly] private bool _guiRightClickInput = false;
    [ReadOnly] private bool _guiRotateInput = false;
    private void Start()
    {
        SceneManager.activeSceneChanged += OnSceneChange;

        Instance.enabled = false;
        if(_playerControls != null)
            _playerControls.Disable();
    }

    public void EnableCharacter()
    {
        Instance.enabled = true;
        if (_playerControls != null)
            _playerControls.Enable();
    }

    private void OnSceneChange(Scene oldScene, Scene newScene)
    {
        Instance.enabled = false;
        if (_playerControls != null)
            _playerControls.Disable();
    }

    private void OnEnable()
    {
        if(_playerControls == null)
        {
            _playerControls = new PlayerControls();

            _playerControls.PlayerActions.Interact.performed += i => _interactionInput = true;
            
            // Locomotion
            _playerControls.PlayerLocomotion.Look.performed += i => mouseDelta = i.ReadValue<Vector2>();
            _playerControls.PlayerLocomotion.Move.performed += i => moveComposite = i.ReadValue<Vector2>();
            _playerControls.PlayerLocomotion.Jump.performed += i => _jumpInput = true;
            _playerControls.PlayerLocomotion.ToggleWalk.performed += i => _walkInput = true;
            _playerControls.PlayerLocomotion.Sprint.performed += i => _sprintInput = true;
            _playerControls.PlayerLocomotion.Sprint.canceled += i => _sprintInput = false;
            _playerControls.PlayerLocomotion.ToggleCrouch.performed += i => _crouchInput = true;
            _playerControls.PlayerLocomotion.LockOn.performed += i => _lockOnInput = true;
            
            // Weapon Actions
            _playerControls.PlayerActions.Roll.performed += i => _rollInput = true;
            
            _playerControls.PlayerActions.LightAttack.performed += i => _lightAttackInput = true;
            _playerControls.PlayerActions.HeavyAttack.performed += i => _heavyAttackInput = true;
            _playerControls.PlayerActions.ChargeAttack.performed += i => _chargeAttackInput = true;
            _playerControls.PlayerActions.ChargeAttack.canceled += i => _chargeAttackInput = false;
            
            _playerControls.PlayerActions.Parry.performed += i => _parryInput = true;
            _playerControls.PlayerActions.Block.performed += i => _blockInput = true;
            _playerControls.PlayerActions.Block.canceled += i => _blockInput = false;
            _playerControls.PlayerActions.Skill.performed += i => _skillInput = true;
            
            // Inventory Actions
            _playerControls.PlayerInventory.ToggleInventory.performed += i => _toggleInventory = true;
            _playerControls.PlayerInventory.ToggleQuickSlot.performed += i => _toggleQuickSlot = true;
            _playerControls.PlayerInventory.ToggleOption.performed += i => _escapeInput = true;
            
            _playerControls.PlayerInventory.SwitchLQuickSlot.performed += i => _switchLQuickSlot = true;
            _playerControls.PlayerInventory.SwitchRQuickSlot.performed += i => _switchRQuickSlot = true;

            _playerControls.PlayerInventory.UseQuickSlotItem.performed += i => _useQuickSlot = true;
            
            // Menu Actions
            _playerControls.UI.ToggleOption.performed += i => _escapeInput = true;
            _playerControls.UI.NextGUI.performed += i => _guiInput = true;
            _playerControls.UI.OpenMap.performed += i => _openMapInput = true;
            _playerControls.UI.Click.performed += i => _guiClickInput = true;
            _playerControls.UI.DoubleClick.performed += i => _guiDoubleClickInput = true;
            _playerControls.UI.RightClick.performed += i => _guiRightClickInput = true;
            _playerControls.UI.Rotate.performed += i => _guiRotateInput = true;
        }

        _playerControls.Enable();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        SceneManager.activeSceneChanged -= OnSceneChange;
    }

    // IF WE MINIMIZE OR LOWER THE WINDOW, STOP ADJUSTING INPUTS
    private void OnApplicationFocus(bool focus)
    {
        if(enabled)
        {
            if(focus)
            {
                _playerControls.Enable();
            }
            else
            {
                _playerControls.Disable();
            }
        }
    }

    private void Update()
    {
        HandleAllInputs();
    }

    private void HandleAllInputs()
    {
        HandleEscape();
        HandleToggleInventory();
        HandleGUI();
        HandleInventoryGUIInput();
        if (playerManager.playerVariableManager.canControl.Value)
        {
            HandleToggleQuickSlot();
            HandleUseQuickSlot();
            
            HandleMoveInput();
            HandleJumpInput();
            HandleToggleWalkInput();
            HandleSprintInput();
            HandleToggleCrouchInput();
            HandleLockOnInput();
            HandleRollInput();
            // Action
            HandleSkillInput();
            HandleParryInput();
            HandleBlockInput();
            HandleLightAttackInput();
            HandleHeavyAttackInput();
            HandleChargingAttackInput();
            HandleSwitchLQuickSlotInput();
            HandleSwitchRQuickSlotInput();
            HandleInteractionInput();
            HandleOpenMap();
        }
        else
        {
            ResetAllInput();
        }
    }

    private void HandleMoveInput()
    {
        movementInputDetected = moveComposite.magnitude > 0;
    }
    
    private void HandleJumpInput()
    {
        if (_jumpInput)
        {
            _jumpInput = false;
            playerManager.playerLocomotionManager.AttemptToJump();
        }
    }

    private void HandleRollInput()
    {
        if (_rollInput)
        {
            _rollInput = false;
            playerManager.playerLocomotionManager.AttemptToRoll();
        }
    }
    private void HandleToggleWalkInput()
    {
        if (_walkInput)
        {
            _walkInput = false;
            playerManager.playerLocomotionManager.AttemptToToggleWalk();
        }
    }
    
    private void HandleSprintInput()
    {
        if (_sprintInput)
        {
            if (_isSprinting) return;
            _isSprinting = true;
            playerManager.playerLocomotionManager.DeactivateCrouch();
            playerManager.playerLocomotionManager.AttemptToActivateSprint();
        }
        else
        {
            if(!_isSprinting) return;
            _isSprinting = false;
            playerManager.playerLocomotionManager.AttemptToDeactivateSprint();
        }
    }
    
    private void HandleToggleCrouchInput()
    {
        if (_crouchInput)
        {
            _crouchInput = false;
            playerManager.playerLocomotionManager.AttemptToToggleCrouch();
            _sprintInput = false;
            _isSprinting = false;
        }
    }
    
    private void HandleLockOnInput()
    {
        if (_lockOnInput)
        {
            _lockOnInput = false;
            playerManager.playerLocomotionManager.AttemptToLockOn();
            playerManager.playerLocomotionManager.AttemptToDeactivateSprint();
        }
    }
    
    private void HandleSkillInput() => HandleInputAttack(_skillInput, AttackType.Skill);
    private void HandleLightAttackInput() => HandleInputAttack(_lightAttackInput, AttackType.LightAttack01);
    private void HandleHeavyAttackInput() => HandleInputAttack(_heavyAttackInput, AttackType.HeavyAttack01);
    private void HandleChargingAttackInput() => HandleInputAttack(_chargeAttackInput, AttackType.ChargeAttack01);
    private void HandleParryInput() => HandleInputAttack(_parryInput, AttackType.Parry);
    private void HandleBlockInput()
    {
       playerManager.playerVariableManager.isBlock.Value = _blockInput;
    }
    
    private void HandleInputAttack(bool input, AttackType actionType)
    {
        if (!input) return;

        // Reset input
        if (actionType == AttackType.Skill) _skillInput = false;
        else if (actionType == AttackType.LightAttack01) _lightAttackInput = false;
        else if (actionType == AttackType.HeavyAttack01) _heavyAttackInput = false;
        else if (actionType == AttackType.ChargeAttack01)
        {
            _chargeAttackInput = false;
            playerManager.playerVariableManager.isCharging.Value = true;
        }
        else if (actionType == AttackType.Parry) _parryInput = false;

        if (playerManager.playerVariableManager.currentEquippedWeaponID.Value == 0) return;

        var weapon = playerManager.playerEquipmentManger.currentEquippedInfoWeapon;
        var action = actionType switch
        {
            AttackType.Skill => weapon.weaponSkill,
            AttackType.LightAttack01 => weapon.lightAttackAction,
            AttackType.HeavyAttack01 => weapon.heavyAttackAction,
            AttackType.ChargeAttack01 => weapon.heavyAttackAction,
            AttackType.Parry => weapon.blockAction,
            _ => throw new ArgumentOutOfRangeException()
        };

        playerManager.playerCombatManager.PerformWeaponBasedAction(action, weapon);
    }

    
    private void HandleSwitchLQuickSlotInput()
    {
        if (_switchLQuickSlot)
        {
            _switchLQuickSlot = false;
            SelectNextQuickSlotItem(FindCurrentSelectQuickSlotItem(), false);
        }
    }

    private void HandleSwitchRQuickSlotInput()
    {
        if (_switchRQuickSlot)
        {
            _switchRQuickSlot = false;
            SelectNextQuickSlotItem(FindCurrentSelectQuickSlotItem(), true);
        }
    }

    private int FindCurrentSelectQuickSlotItem()
    {
        int index = 0;
        foreach (var itemID in playerManager.playerVariableManager.currentQuickSlotIDList.Value)
        {
            if (playerManager.playerVariableManager.currentSelectQuickSlotItem.Value == itemID)
            {
                return index;
            }

            index++;
        }
        return 0;
    }

    private void SelectNextQuickSlotItem(int curIndex, bool isRight)
    {
        int maxCount = playerManager.playerVariableManager.currentQuickSlotIDList.Count;
        for (int i = 0; i < maxCount; i++)
        {
            int searchIndex = (isRight ? (i + curIndex) : (curIndex - i + maxCount)) % maxCount;

            if (playerManager.playerVariableManager.currentSelectQuickSlotItem.Value !=
                playerManager.playerVariableManager.currentQuickSlotIDList[searchIndex])
            {
                playerManager.playerVariableManager.currentSelectQuickSlotItem.Value =
                    playerManager.playerVariableManager.currentQuickSlotIDList[searchIndex];
                return;
            }
        }
    }
    
    private void HandleInteractionInput()
    {
        if (_interactionInput)
        {
            _interactionInput = false;
            
           playerManager.playerInteractionManager.Interact();
        }
    }

    private void HandleOpenMap()
    {
        if (_openMapInput)
        {
            _openMapInput = false;
            
            GUIController.Instance.OpenMap();
        }
    }

    private void HandleGUI()
    {
        if (_guiInput)
        {
            _guiInput = false;
            
            GUIController.Instance.HandleNextGUI();
        }
    }
    
    private void HandleEscape()
    {
        if (_escapeInput)
        {
            _escapeInput = false;
            GUIController.Instance.HandleEscape();
        }
    }

    private void HandleToggleInventory()
    {
        if (_toggleInventory)
        {
            _toggleInventory = false;
            GUIController.Instance.HandleTab();
        }
    }

    private void HandleInventoryGUIInput()
    {
        InventoryController inventoryController = GetInventoryController();
        if (inventoryController != null)
        {
            HandleInventoryGUIRightClick(inventoryController);
            //HandleInventoryGUIDoubleClick(inventoryController);
            HandleInventoryGUIClick(inventoryController);
            HandleInventoryGUIRotate(inventoryController);
        }
    }

    private InventoryController GetInventoryController()
    {
        if (GUIController.Instance != null && GUIController.Instance.inventoryGUIManager != null &&
            GUIController.Instance.inventoryGUIManager.inventoryController != null)
        {
            return GUIController.Instance.inventoryGUIManager.inventoryController;
        }
        else
        {
            return null;
        }
    }

    private void HandleInventoryGUIRightClick(InventoryController inventoryController)
    {
        if (inventoryController.isActive &&_guiRightClickInput)
        {
            _guiRightClickInput = false;
            inventoryController.RightMouseButtonPress();
        }
    }

    private void HandleInventoryGUIDoubleClick(InventoryController inventoryController)
    {
        if (inventoryController.isActive && _guiDoubleClickInput)
        {
            _guiDoubleClickInput = false;
            _guiClickInput = false;
            inventoryController.RightMouseButtonPress();
        }
    }
    
    private void HandleInventoryGUIClick(InventoryController inventoryController)
    {
        if (inventoryController.isActive && _guiClickInput)
        {
            _guiClickInput = false;
            inventoryController.LeftMouseButtonPress();
        }
    }

    private void HandleInventoryGUIRotate(InventoryController inventoryController)
    {
        if (inventoryController.isActive && _guiRotateInput)
        {
            _guiRotateInput = false;
            inventoryController.RotateItem();
        }
    }

    private void HandleToggleQuickSlot()
    {
        if (_toggleQuickSlot)
        {
            _toggleQuickSlot = false;
            GUIController.Instance.playerUIHudManager.playerUIQuickSlotManager.ToggleQuickSlotItem();
        }
    }

    private void HandleUseQuickSlot()
    {
        if (_useQuickSlot)
        {
            _useQuickSlot = false;
            
            playerManager.playerItemConsumeManager.UseQuickSlotItem();
        }
    }

    private void ResetAllInput()
    {
        cameraVerticalInput = 0;
        cameraHorizontalInput = 0;
        movementInputDetected = false;
        movementInputDuration = 0;
        
        _isSprinting = false;
        _jumpInput = false;
        _walkInput = false;
        _sprintInput = false;
        _crouchInput = false;
        _lockOnInput = false;
        
        _rollInput = false;
        
        _interactionInput = false;
        
        _lightAttackInput = false;
        _heavyAttackInput = false;
        
        _blockInput = false;
        
        _toggleQuickSlot = false;
        
        _switchLQuickSlot = false;
        _switchRQuickSlot = false;
        
        _useQuickSlot = false;
    }

    public void SetControlActive(bool isActive)
    {
        if (isActive)
        {
            // 게임 활성화
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            
            if(playerManager != null)
                playerManager.playerVariableManager.canControl.Value = true;
        }
        else
        {
            // UI 활성화 
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            if(playerManager != null)
                playerManager.playerVariableManager.canControl.Value = false;
        }
    }
    
    public void CalculateInput()
    {
        Vector3 moveDirection = Vector3.zero;
        if (movementInputDetected)
        {
            if (movementInputDuration == 0)
            {
                CLVM.movementInputTapped = true;
            }
            else if (movementInputDuration > 0 && movementInputDuration < buttonHoldThreshold)
            {
                CLVM.movementInputTapped = false;
                CLVM.movementInputPressed = true;
                CLVM.movementInputHeld = false;
            }
            else
            {
                CLVM.movementInputTapped = false;
                CLVM.movementInputPressed = false;
                CLVM.movementInputHeld = true;
            }

            movementInputDuration += Time.deltaTime;
            moveDirection = (PlayerCameraController.Instance.GetCameraForwardZeroedYNormalized() * moveComposite.y) +
                            (PlayerCameraController.Instance.GetCameraRightZeroedYNormalized() * moveComposite.x);
        }
        else
        {
            movementInputDuration = 0;
            CLVM.movementInputTapped = false;
            CLVM.movementInputPressed = false;
            CLVM.movementInputHeld = false;
        }

        CLVM.moveDirection = moveDirection;
    }
}

