using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler_Locomotion : MonoBehaviour, InputHandlerManager.IInputHandler
{
    private PlayerManager _playerManager;
    private CharacterLocomotionVariableManager CLVM => _playerManager.characterVariableManager.CLVM;

    private bool _isSprinting = false;
    private float _targetSize = 3.5f;
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
        playerControls.PlayerLocomotion.Crouch.performed += OnCrouchPerformed;
        playerControls.PlayerLocomotion.Crouch.canceled += OnCrouchPerformed;
        playerControls.PlayerLocomotion.LockOn.performed += OnLockOnPerformed;
        playerControls.PlayerLocomotion.LockOn.canceled += OnLockOnPerformed;
        
        playerControls.PlayerActions.Enable();
        playerControls.PlayerActions.Interact.performed += OnInteractPerformed;
        playerControls.PlayerActions.Roll.performed += OnRollPerformed;
        
        playerControls.CamControl.Enable();
        playerControls.CamControl.Zoom.performed += OnZoom;

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
        playerControls.PlayerLocomotion.Crouch.performed -= OnCrouchPerformed;
        playerControls.PlayerLocomotion.Crouch.canceled -= OnCrouchPerformed;
        playerControls.PlayerLocomotion.LockOn.performed -= OnLockOnPerformed;
        playerControls.PlayerLocomotion.LockOn.canceled -= OnLockOnPerformed;
        
        playerControls.PlayerActions.Interact.performed -= OnInteractPerformed;
        playerControls.PlayerActions.Roll.performed -= OnRollPerformed;
        
        
        playerControls.CamControl.Zoom.performed -= OnZoom;
        
        playerControls.PlayerLocomotion.Disable();
        playerControls.PlayerActions.Disable();
        playerControls.CamControl.Disable();

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
            UpdateZoom();
        }
    }

    private void UpdateMoveDir()
    {
        Vector3 moveDir;
        if (CLVM.isLockedOn)
        {
            moveDir =
                (PlayerCameraController.Instance.GetPlayerOrthographicDir * CLVM.moveComposite.y) +
                (PlayerCameraController.Instance.GetPlayerRightDir() * CLVM.moveComposite.x);
        }
        else
        {
            moveDir =
                (PlayerCameraController.Instance.GetCamForward() * CLVM.moveComposite.y) +
                (PlayerCameraController.Instance.GetCamRight() * CLVM.moveComposite.x);
        }
        CLVM.moveDirection = moveDir;
    }

    private void UpdateZoom()
    {
        PlayerCameraController.Instance.SetOrthographicTargetSize(_targetSize);
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
        Debug.Log($"[BTN INPUT] can Control : {_playerManager.playerVariableManager.canControl.Value}");
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
    
    private void OnCrouchPerformed(InputAction.CallbackContext context)
    {
        if (_playerManager.playerVariableManager.canControl.Value)
        {
            var value = context.ReadValue<float>();
            if (value >= 0.5f)
            {
                _playerManager.playerLocomotionManager.AttemptToToggleCrouch();
                _isSprinting = false;
            }
            else
            {
                _playerManager.playerLocomotionManager.AttemptToToggleCrouch();
            }
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
    
    public void OnZoom(InputAction.CallbackContext context)
    {
        Vector2 scrollValue = context.ReadValue<Vector2>();
    
        _targetSize -= scrollValue.y * 0.1f;
        _targetSize = Mathf.Clamp(_targetSize, 1, 10);
    }

    #endregion
}