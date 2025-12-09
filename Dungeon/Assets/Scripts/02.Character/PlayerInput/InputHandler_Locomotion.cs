using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler_Locomotion : MonoBehaviour, InputHandlerManager.IInputHandler
{
    private PlayerManager _playerManager;
    private CharacterLocomotionVariableManager CLVM => _playerManager.characterVariableManager.CLVM;
    
    private PlayerControls _playerControls;

    public float movementInputDuration;

    private bool _isSprinting = false;
    
    private readonly float _buttonHoldThreshold = 0.05f;

    private void Awake()
    {
        _playerControls = new PlayerControls();
    }

    public void SetPlayer(PlayerManager playerManager)
    {
        _playerManager = playerManager;
    }
    
    public void Register()
    {
        Debug.Log("[Register] InputHandler_Locomotion");
        _playerControls.PlayerLocomotion.Enable();
        _playerControls.PlayerLocomotion.Move.started += OnMoveStarted;
        _playerControls.PlayerLocomotion.Move.performed += OnMovePerformed;
        _playerControls.PlayerLocomotion.Move.canceled += OnMoveCanceled;
        _playerControls.PlayerLocomotion.Jump.performed += OnJumpPerformed;
        _playerControls.PlayerLocomotion.ToggleWalk.performed += OnToggleWalkPerformed;
        _playerControls.PlayerLocomotion.Sprint.performed += OnSprintPerformed;
        _playerControls.PlayerLocomotion.Sprint.canceled += OnSprintCanceled;
        _playerControls.PlayerLocomotion.ToggleCrouch.performed += OnToggleCrouchPerformed;
        _playerControls.PlayerLocomotion.LockOn.performed += OnLockOnPerformed;
        
        _playerControls.PlayerActions.Enable();
        _playerControls.PlayerActions.Interact.performed += OnInteractPerformed;
        _playerControls.PlayerActions.Roll.performed += OnRollPerformed;
    }

    public void Unregister()
    {
        Debug.Log("[Unregister] InputHandler_Locomotion");
        _playerControls.PlayerLocomotion.Move.started -= OnMoveStarted;
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
        
        _playerControls.PlayerLocomotion.Disable();
        _playerControls.PlayerActions.Disable();
    }
    
    public void EnableInput()
    {
        Debug.Log("[Enable] InputHandler_Locomotion");
        _playerControls.PlayerLocomotion.Enable();
        _playerControls.PlayerActions.Enable();
    }

    public void DisableInput()
    {
        Debug.Log("[Disable] InputHandler_Locomotion");
        _playerControls.PlayerLocomotion.Disable();
        _playerControls.PlayerActions.Disable();
    }
    
    private void OnDestroy()
    {
        if (InputHandlerManager.Instance != null)
        {
            InputHandlerManager.Instance.UnregisterAndDisableHandler(this);
        }
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
        if (_playerControls.PlayerLocomotion.Move.IsInProgress())
        {
            movementInputDuration += Time.deltaTime;
            
            CLVM.movementInputTapped = movementInputDuration == 0;
            CLVM.movementInputPressed = movementInputDuration > 0 && movementInputDuration < _buttonHoldThreshold;
            CLVM.movementInputHeld = movementInputDuration >= _buttonHoldThreshold;

            Vector3 moveDirection = 
                (PlayerCameraController.Instance.GetCameraForwardZeroedYNormalized() * CLVM.moveComposite.y) +
                (PlayerCameraController.Instance.GetCameraRightZeroedYNormalized() * CLVM.moveComposite.x);
                
            CLVM.moveDirection = moveDirection;
        }
    }

    #region Event CallBack

    // Locomotion

    private void OnMoveStarted(InputAction.CallbackContext context)
    {
        movementInputDuration = 0;
        CLVM.movementInputTapped = true;
    }
    private void OnMovePerformed(InputAction.CallbackContext context) => CLVM.moveComposite = context.ReadValue<Vector2>();
    
    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        CLVM.moveComposite = Vector2.zero;
        movementInputDuration = 0;
        CLVM.movementInputTapped = false;
        CLVM.movementInputPressed = false;
        CLVM.movementInputHeld = false;
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

    #endregion
    
    
}