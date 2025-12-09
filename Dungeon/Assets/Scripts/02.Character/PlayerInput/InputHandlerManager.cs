using System;
using System.Collections.Generic;
using UnityEngine;

public enum InputMode { Exploration, Combat, OpenUI, Exit }

public class InputHandlerManager : Singleton<InputHandlerManager>
{
    private PlayerManager _playerManager;
    private readonly HashSet<IInputHandler> _activeHandlers = new HashSet<IInputHandler>();
    private InputMode _lastInputMode = InputMode.Exit;
    private InputMode _curInputMode;
    [SerializeField] private InputHandler_Locomotion movementHandler;
    [SerializeField] private InputHandler_CombatSwipe combatHandler;
    [SerializeField] private InputHandler_UI uiHandler;
    
    public interface IInputHandler
    {
        void SetPlayer(PlayerManager playerManager);
        void EnableInput();
        void DisableInput();
        void Register();
        void Unregister();
    }

    private void Start()
    {
        movementHandler.Register();
        movementHandler.DisableInput();
        combatHandler.Register();
        combatHandler.DisableInput();
        uiHandler.Register();
        uiHandler.DisableInput();
    }
    
    public void SetPlayer(PlayerManager playerManager)
    {
        _playerManager = playerManager;

        foreach (var handler in _activeHandlers)
        {
            handler.SetPlayer(_playerManager);
        }
    }

    private void EnableHandler(IInputHandler handler)
    {
        if(handler == null) return;
        if (_activeHandlers.Add(handler))
        {
            handler.EnableInput();
            handler.SetPlayer(_playerManager);
        }
    }

    private void DisableHandler(IInputHandler handler)
    {
        if (handler == null) return;
        if (_activeHandlers.Remove(handler))
        {
            handler.DisableInput();
        }
    }

    public void RegisterAndEnableHandler(IInputHandler handler)
    {
        if(handler == null) return;
        if (_activeHandlers.Add(handler))
        {
            handler.EnableInput();
            handler.SetPlayer(_playerManager);
        }
    }
    
    public void UnregisterAndDisableHandler(IInputHandler handler)
    {
        if(handler == null) return;
        if (_activeHandlers.Remove(handler))
        {
            handler.DisableInput();
        }
    }

    public void SetLastInputMode()
    {
        SetInputMode(_lastInputMode);
    }
    
    public void SetInputMode(InputMode mode)
    {
        Debug.Log($"Input Mode : {mode}");
        _lastInputMode = _curInputMode;
        _curInputMode = mode;
        switch (mode)
        {
            case InputMode.Exploration:
                SetControlActive(true);
                EnableHandler(movementHandler); 
                DisableHandler(combatHandler); 
                EnableHandler(uiHandler);
                break;
            case InputMode.Combat:
                SetControlActive(false);
                DisableHandler(movementHandler); 
                EnableHandler(combatHandler); 
                EnableHandler(uiHandler);
                break;
            case InputMode.OpenUI:
                SetControlActive(false);
                ResetLocomotion();
                DisableHandler(movementHandler); 
                DisableHandler(combatHandler); 
                EnableHandler(uiHandler);
                break;
            case InputMode.Exit:
                SetControlActive(false);
                UnregisterAndDisableHandler(movementHandler); 
                UnregisterAndDisableHandler(combatHandler); 
                UnregisterAndDisableHandler(uiHandler);
                break;
        }
    }
    
    private void SetControlActive(bool isActive)
    {
        //Debug.Log($"SetControlActive : {isActive}");
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

    private void ResetLocomotion()
    {
        _playerManager.playerVariableManager.CLVM.moveDirection = Vector3.zero;
    }

    private void OnDisable()
    {
        SetInputMode(InputMode.Exit);
    }
}
