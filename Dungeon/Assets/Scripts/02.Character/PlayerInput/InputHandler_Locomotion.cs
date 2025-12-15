using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler_Locomotion : MonoBehaviour, InputHandlerManager.IInputHandler
{
    private PlayerManager _playerManager;
    private CharacterLocomotionVariableManager CLVM => _playerManager.characterVariableManager.CLVM;

    private bool _isSprinting = false;

    private bool _isEnable = false;

    public void SetPlayer(PlayerManager playerManager)
    {
        _playerManager = playerManager;
    }
    
    public void Register(PlayerControls playerControls)
    {
        Debug.Log("[Register] InputHandler_Locomotion");
        playerControls.PlayerLocomotion.Enable();
        playerControls.PlayerLocomotion.Move.performed += OnMovePerformed;
        playerControls.PlayerLocomotion.Look.performed += OnLookPerformed;
        playerControls.PlayerLocomotion.Look.canceled += OnLookPerformed;
        playerControls.PlayerLocomotion.Jump.performed += OnJumpPerformed;
        playerControls.PlayerLocomotion.ToggleWalk.performed += OnToggleWalkPerformed;
        playerControls.PlayerLocomotion.Sprint.performed += OnSprintPerformed;
        playerControls.PlayerLocomotion.Sprint.canceled += OnSprintCanceled;
        playerControls.PlayerLocomotion.ToggleCrouch.performed += OnToggleCrouchPerformed;
        playerControls.PlayerLocomotion.LockOn.performed += OnLockOnPerformed;
        playerControls.PlayerLocomotion.LockOn.canceled += OnLockOnPerformed;
        
        playerControls.PlayerActions.Enable();
        playerControls.PlayerActions.Interact.performed += OnInteractPerformed;
        playerControls.PlayerActions.Roll.performed += OnRollPerformed;

        _isEnable = true;
    }

    public void Unregister(PlayerControls playerControls)
    {
        Debug.Log("[Unregister] InputHandler_Locomotion");
        playerControls.PlayerLocomotion.Move.performed -= OnMovePerformed;
        playerControls.PlayerLocomotion.Look.performed -= OnLookPerformed;
        playerControls.PlayerLocomotion.Look.canceled -= OnLookPerformed;
        playerControls.PlayerLocomotion.Jump.performed -= OnJumpPerformed;
        playerControls.PlayerLocomotion.ToggleWalk.performed -= OnToggleWalkPerformed;
        playerControls.PlayerLocomotion.Sprint.performed -= OnSprintPerformed;
        playerControls.PlayerLocomotion.Sprint.canceled -= OnSprintCanceled;
        playerControls.PlayerLocomotion.ToggleCrouch.performed -= OnToggleCrouchPerformed;
        playerControls.PlayerLocomotion.LockOn.performed -= OnLockOnPerformed;
        playerControls.PlayerLocomotion.LockOn.canceled -= OnLockOnPerformed;
        
        playerControls.PlayerActions.Interact.performed -= OnInteractPerformed;
        playerControls.PlayerActions.Roll.performed -= OnRollPerformed;
        
        playerControls.PlayerLocomotion.Disable();
        playerControls.PlayerActions.Disable();

        _isEnable = false;
    }
    
    private void OnDestroy()
    {
        if (InputHandlerManager.Instance != null)
        {
            InputHandlerManager.Instance.UnregisterAndDisableHandler(this);
        }
    }
    
    private void Update()
    {
        if (_isEnable)
        {
            UpdateMoveDir();
        }
    }

    private void UpdateMoveDir()
    {
        Vector3 moveDir;
        if (CLVM.isLockedOn)
        {
            moveDir =
                (PlayerCameraController.Instance.GetPlayerDir * CLVM.moveComposite.y) +
                (PlayerCameraController.Instance.GetPlayerRightDir() * CLVM.moveComposite.x);
        }
        else
        {
            moveDir =
                (Vector3.forward * CLVM.moveComposite.y) +
                (Vector3.right * CLVM.moveComposite.x);
        }
        CLVM.moveDirection = moveDir;
    }

    #region Event CallBack

    // Locomotion
    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        var inputVector = context.ReadValue<Vector2>();
        CLVM.moveComposite = inputVector;
        CLVM.movementInputHeld = inputVector.magnitude > 0;
    } 
    
    private void OnLookPerformed(InputAction.CallbackContext context)
    {
        Vector2 mousePositionInput = context.ReadValue<Vector2>();
        PlayerCameraController.Instance.SetMousePosition(mousePositionInput);
    }

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
        var isPressed = context.ReadValue<float>();
        Debug.Log($"isPress : {isPressed}");
        CLVM.isLockedOn = (isPressed >= 0.5f);
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

    #endregion
    
    
}