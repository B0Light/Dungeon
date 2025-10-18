using System.Collections.Generic;
using UnityEngine;

public enum InputMode { Exploration, Combat, UI_Open }

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
        var combatHandler = FindAnyObjectByType<InputHandler_Combat>();

        switch (mode)
        {
            case InputMode.Exploration:
                UnregisterAndDisableHandler(combatHandler);
                RegisterAndEnableHandler(movementHandler); 
                break;
            case InputMode.Combat:
                RegisterAndEnableHandler(combatHandler);
                RegisterAndEnableHandler(movementHandler); // 전투 중에도 이동은 유지한다고 가정
                break;
            case InputMode.UI_Open:
                // 모든 게임플레이 입력 비활성화 후 UI 입력만 활성화하는 로직 구현 가능
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
