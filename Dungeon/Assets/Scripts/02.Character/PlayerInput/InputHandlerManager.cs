using System.Collections.Generic;
using UnityEngine;

public enum InputMode { Exploration, CombatBuild, OpenUI }

public class InputHandlerManager : Singleton<InputHandlerManager>
{
    private PlayerManager _playerManager;
    private readonly HashSet<IInputHandler> _activeHandlers = new HashSet<IInputHandler>();
    
    public void SetPlayer(PlayerManager playerManager)
    {
        _playerManager = playerManager;

        foreach (var handler in _activeHandlers)
        {
            handler.SetPlayer(_playerManager);
        }
    }
    
    public interface IInputHandler
    {
        void SetPlayer(PlayerManager playerManager);
        void EnableInput();
        void DisableInput();
    }
    
    public void RegisterAndEnableHandler(IInputHandler handler)
    {
        if (_activeHandlers.Add(handler))
        {
            handler.EnableInput();
            handler.SetPlayer(_playerManager);
            Debug.Log($"Input Handler Registered and Enabled: {handler.GetType().Name}");
        }
    }
    
    public void UnregisterAndDisableHandler(IInputHandler handler)
    {
        if (_activeHandlers.Remove(handler))
        {
            handler.DisableInput();
            Debug.Log($"Input Handler Unregistered and Disabled: {handler.GetType().Name}");
        }
    }
    
    public void SetInputMode(InputMode mode)
    {
        var movementHandler = FindAnyObjectByType<InputHandler_Locomotion>();
        // combat build
        var uiHandler = FindAnyObjectByType<InputHandler_UI>();

        switch (mode)
        {
            case InputMode.Exploration:
                RegisterAndEnableHandler(movementHandler); 
                RegisterAndEnableHandler(uiHandler);
                break;
            case InputMode.CombatBuild:
                UnregisterAndDisableHandler(movementHandler); 
                RegisterAndEnableHandler(uiHandler);
                break;
            case InputMode.OpenUI:
                UnregisterAndDisableHandler(movementHandler); 
                RegisterAndEnableHandler(uiHandler);
                break;
        }
    }
    
    public void SetControlActive(bool isActive)
    {
        Debug.Log($"SetControlActive : {isActive}");
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
}
